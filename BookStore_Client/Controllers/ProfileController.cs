using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using BookStore_Client.Models;
using BookStore_API.Domain.DTO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using BookStore_Client.Domain.DTO;

namespace BookStore_Client.Controllers
{
    [Route("Profile")] // Đơn giản hóa route, bỏ "api/Profile/[controller]"
    public class ProfileController : Controller // Không dùng [ApiController] để hỗ trợ trả về View
    {
        private readonly string _apiBaseUrl = "https://localhost:7202/api/User";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public ProfileController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var userDataJson = HttpContext.Session.GetString("customerInfo");
            if (string.IsNullOrEmpty(userDataJson) && string.IsNullOrEmpty(HttpContext.Session.GetString("JWTToken")))
            {
                return RedirectToAction("Login", "User");
            }

            User user;
            if (!string.IsNullOrEmpty(userDataJson))
            {
                user = JsonConvert.DeserializeObject<User>(userDataJson);
            }
            else
            {
                var token = HttpContext.Session.GetString("JWTToken");
                var userInfo = UserController.DecodeJwtToken(token); // Gọi phương thức tĩnh từ UserController
                user = new User
                {
                    UserID = Convert.ToInt32(userInfo.GetValueOrDefault(System.Security.Claims.ClaimTypes.NameIdentifier)),
                    Username = HttpContext.Session.GetString("Username"),
                    Email = Convert.ToString(userInfo.GetValueOrDefault(System.Security.Claims.ClaimTypes.Email)),
                    Role = HttpContext.Session.GetInt32("Role").GetValueOrDefault()
                };
            }

            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"{_apiBaseUrl}/by-email/{user.Email}";
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var updatedUser = JsonConvert.DeserializeObject<User>(jsonResponse);
                HttpContext.Session.SetString("customerInfo", JsonConvert.SerializeObject(updatedUser));
                return View(updatedUser);
            }

            return View(user);
        }

        [HttpGet("edit-profile/{userId}")]
        public async Task<IActionResult> EditProfile(int userId)
        {
            var userDataJson = HttpContext.Session.GetString("customerInfo");
            if (string.IsNullOrEmpty(userDataJson))
            {
                return RedirectToAction("Login", "User");
            }

            var user = JsonConvert.DeserializeObject<User>(userDataJson);

            if (user.UserID != userId)
            {
                return Unauthorized("You can only edit your own profile.");
            }

            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"{_apiBaseUrl}/{userId}";
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var updatedUser = JsonConvert.DeserializeObject<User>(jsonResponse);
                return View(updatedUser);
            }

            return View(user);
        }

        [HttpPost("edit-profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User updatedUser, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation Error: {error}");
                }
                return View(updatedUser);
            }

            var userDataJson = HttpContext.Session.GetString("customerInfo");
            if (string.IsNullOrEmpty(userDataJson))
            {
                return RedirectToAction("Login", "User");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userDataJson);
            if (currentUser.UserID != updatedUser.UserID)
            {
                return Unauthorized("You can only edit your own profile.");
            }

            string imageUrl = updatedUser.ImageUrl ?? string.Empty;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "File size must be less than 5MB.");
                    return View(updatedUser);
                }

                if (!ImageFile.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("ImageFile", "Please upload a valid image file.");
                    return View(updatedUser);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (!string.IsNullOrEmpty(updatedUser.ImageUrl))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", updatedUser.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/uploads/{fileName}";
            }

            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"{_apiBaseUrl}/{currentUser.UserID}";
            var userDTO = new UserDTO
            {
                UserID = currentUser.UserID,
                Username = updatedUser.Username,
                FullName = updatedUser.FullName,
                Email = updatedUser.Email,
                Phone = updatedUser.Phone,
                Address = updatedUser.Address,
                Gender = updatedUser.Gender,
                ImageUrl = imageUrl
            };

            Console.WriteLine($"Sending to API: {JsonConvert.SerializeObject(userDTO)}");

            var response = await httpClient.PutAsJsonAsync(apiUrl, userDTO);

            if (response.IsSuccessStatusCode)
            {
                updatedUser.ImageUrl = imageUrl;
                updatedUser.CreateAt = currentUser.CreateAt;
                updatedUser.IsDelete = currentUser.IsDelete;
                updatedUser.Role = currentUser.Role;
                var updatedUserJson = JsonConvert.SerializeObject(updatedUser);
                HttpContext.Session.SetString("customerInfo", updatedUserJson);
                HttpContext.Session.SetString("Username", updatedUser.Username ?? updatedUser.FullName);

                return RedirectToAction("Profile");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to update profile: {errorContent}");
            Console.WriteLine($"API Error: {errorContent}");
            return View(updatedUser);
        }
    }
}