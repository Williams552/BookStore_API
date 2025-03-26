using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BookStore_Client.Controllers
{
    [Route("Profile")] // Đơn giản hóa route, bỏ "api/Profile/[controller]"
    public class ProfileController : Controller // Không dùng [ApiController] để hỗ trợ trả về View
    {
        private readonly string _apiBaseUrl = "https://localhost:7202/api/User";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "https://localhost:7202/api/User";

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
            var client = _httpClientFactory.CreateClient();
            try
            try
                int userId = HttpContext.Session.GetInt32("UserId") ?? 1; // Lấy từ session thay vì cố định
                var response = await client.GetAsync($"{_apiBaseUrl}/{userId}");
                var response = await client.GetAsync($"{_apiBaseUrl}/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<User>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return View(user);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ViewBag.ErrorMessage = $"User with ID {userId} not found.";
                    return View();
                    return View();
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

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 1; // Lấy từ session
                var response = await client.GetAsync($"{_apiBaseUrl}/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<User>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    // Truyền thông tin user qua ViewBag
                    ViewBag.UserImageUrl = user.ImageUrl;
                    ViewBag.UserFullName = user.FullName;
                    ViewBag.UserEmail = user.Email;
                }
                else
                {
                    ViewBag.UserImageUrl = null;
                    ViewBag.UserFullName = "N/A";
                    ViewBag.UserEmail = "N/A";
                }
            }
            catch (Exception ex)
            {
                ViewBag.UserImageUrsl = null;
                ViewBag.UserFullName = "N/A";
                ViewBag.UserEmail = "N/A";
            }

            return View(new ChangePasswordViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp với mật khẩu mới.");
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 1; // Lấy từ session thay vì cố định
                var request = new
                {
                    OldPassword = model.OldPassword,
                    NewPassword = model.NewPassword,
                    ConfirmPassword = model.ConfirmPassword
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiBaseUrl}/{userId}/change-password", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var error = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                    ModelState.AddModelError(string.Empty, error?.Message ?? "Đã xảy ra lỗi khi đổi mật khẩu.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                return View(model);
            }
        }
    }

    // ViewModel cho đổi mật khẩu
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
        [MinLength(5, ErrorMessage = "Mật khẩu mới phải có ít nhất 5 ký tự.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu mới.")]
        public string ConfirmPassword { get; set; }
    }

    // DTO cho phản hồi lỗi từ API
    public class ErrorResponse
    {
        public string Message { get; set; }
    }


    //something to commit?
}