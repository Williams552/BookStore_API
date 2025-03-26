using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using BookStore_Client.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text.Json;
using BookStore_Client.Domain.DTO;
using BookStore_Client.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace BookStore_Client.Controllers
{
    [Route("api/Auth/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly string _apiBaseUrl = "https://localhost:7202/api/User";
        private readonly HttpClient _httpClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserController> _logger;

        public UserController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, HttpClient httpClient, ILogger<UserController> logger)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);
            _logger = logger;
        }

        private IActionResult CheckAdminAccess()
        {
            var role = HttpContext.Session.GetInt32("Role");

            if (!role.HasValue)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "User");
            }

            if (role != 1)
            {
                // Nếu không phải admin, chuyển hướng về trang Home Page
                return Redirect("https://localhost:7106/");
            }

            return null; // Cho phép tiếp tục nếu là admin
        }

        public static Dictionary<string, string> DecodeJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                var requestUrl = $"{_apiBaseUrl}/login?username={user.Username}&password={user.Password}";
                var content = new StringContent("", Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var jsonObject = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
                    string token = jsonObject.GetProperty("token").GetString();
                    HttpContext.Session.SetString("JWTToken", token);
                    var userInfo = DecodeJwtToken(token);
                    int userId = Convert.ToInt32(userInfo.GetValueOrDefault(ClaimTypes.NameIdentifier));
                    int role = Convert.ToInt32(userInfo.GetValueOrDefault(ClaimTypes.Role));
                    string email = Convert.ToString(userInfo.GetValueOrDefault(ClaimTypes.Email));
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Email", email);
                    HttpContext.Session.SetInt32("UserId", userId);
                    HttpContext.Session.SetInt32("Role", role);

                    // Phân quyền sau khi đăng nhập
                    if (role == 1) // Admin
                    {
                        return Redirect("https://localhost:7106/Book");
                    }
                    else if (role == 2 || role == 3) // Người dùng thông thường
                    {
                        return Redirect("https://localhost:7106/");
                    }
                    else
                    {
                        // Nếu Role không hợp lệ, đăng xuất và chuyển về trang đăng nhập
                        HttpContext.Session.Clear();
                        ModelState.AddModelError(string.Empty, "Vai trò không hợp lệ!");
                        return RedirectToAction("Login", "User");
                    }
                }
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng!");
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Tài khoản hoặc mật khẩu không đúng!");
                return RedirectToAction("Login", "User");
            }
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction(nameof(Login));
            }

            var claims = authenticateResult.Principal.Identities
                .FirstOrDefault()?.Claims.ToList();

            var emailClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var firstName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
            var lastName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";

            if (string.IsNullOrEmpty(emailClaim))
            {
                return RedirectToAction(nameof(Login));
            }

            var httpClient = _httpClientFactory.CreateClient();
            var apiBaseUrl = "https://localhost:7202/api/Auth";
            var request = new
            {
                Email = emailClaim,
                FirstName = firstName,
                LastName = lastName
            };

            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{apiBaseUrl}/google-login", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                if (result.success == true)
                {
                    var user = result.user.ToObject<User>();

                    HttpContext.Session.Remove("customerInfo");
                    Microsoft.AspNetCore.Http.SessionExtensions.SetString(HttpContext.Session, "Username", user.Username);
                    Microsoft.AspNetCore.Http.SessionExtensions.SetInt32(HttpContext.Session, "UserId", user.UserID);
                    string userDataJson = JsonConvert.SerializeObject(user);
                    Microsoft.AspNetCore.Http.SessionExtensions.SetString(HttpContext.Session, "customerInfo", userDataJson);

                    // Phân quyền sau khi đăng nhập bằng Google
                    if (user.Role == 1) // Admin
                    {
                        return Redirect("https://localhost:7106/Book");
                    }
                    else if (user.Role == 2 || user.Role == 3) // Người dùng thông thường
                    {
                        return Redirect("https://localhost:7106/");
                    }
                    else
                    {
                        // Nếu Role không hợp lệ, đăng xuất và chuyển về trang đăng nhập
                        HttpContext.Session.Clear();
                        return RedirectToAction(nameof(Login));
                    }
                }
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
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
                // Lấy thông tin từ các key khác nếu không có customerInfo
                var token = HttpContext.Session.GetString("JWTToken");
                var userInfo = DecodeJwtToken(token);
                user = new User
                {
                    UserID = Convert.ToInt32(userInfo.GetValueOrDefault(ClaimTypes.NameIdentifier)),
                    Username = HttpContext.Session.GetString("Username"),
                    Email = Convert.ToString(userInfo.GetValueOrDefault(ClaimTypes.Email)),
                    Role = HttpContext.Session.GetInt32("Role").GetValueOrDefault()
                };
            }

            // Gọi API để lấy thông tin mới nhất
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"https://localhost:7202/api/User/by-email/{user.Email}";
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

            var currentUser = JsonConvert.DeserializeObject<User>(userDataJson);
            if (currentUser.UserID != userId)
            {
                return Unauthorized("You can only edit your own profile.");
            }

            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"https://localhost:7202/api/User/{userId}";
            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch user data: {await response.Content.ReadAsStringAsync()}");
                return StatusCode((int)response.StatusCode, "Error fetching user data.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<User>(jsonResponse);

            // Tạo model cho view
            var editModel = new UserEditModel
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender
            };

            // Truyền thông tin user qua ViewBag để hiển thị
            ViewBag.CurrentUser = user;

            return View(editModel);
        }

        [HttpPost("edit-profile/{userId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int userId, [FromForm] UserEditModel editModel, IFormFile? ImageFile)
        {
            var userDataJson = HttpContext.Session.GetString("customerInfo");
            if (string.IsNullOrEmpty(userDataJson))
            {
                return RedirectToAction("Login", "User");
            }

            var currentUser = JsonConvert.DeserializeObject<User>(userDataJson);
            if (currentUser.UserID != userId)
            {
                return Unauthorized("You can only edit your own profile.");
            }

            string imageUrl = currentUser.ImageUrl;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "File size must be less than 5MB.");
                    ViewBag.CurrentUser = currentUser;
                    return View(editModel);
                }
                if (!ImageFile.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("ImageFile", "Please upload a valid image file.");
                    ViewBag.CurrentUser = currentUser;
                    return View(editModel);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                imageUrl = $"/uploads/{fileName}";
            }

            var userDTO = new UserDTO
            {
                UserID = userId,
                Username = currentUser.Username,
                FullName = editModel.FullName,
                Email = currentUser.Email,
                Phone = editModel.Phone,
                Address = editModel.Address,
                Gender = editModel.Gender,
                ImageUrl = imageUrl
            };

            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"https://localhost:7202/api/User/{userId}";
            Console.WriteLine($"Sending to API: {JsonConvert.SerializeObject(userDTO)}");
            var response = await httpClient.PutAsJsonAsync(apiUrl, userDTO);

            if (response.IsSuccessStatusCode)
            {
                var updatedUser = new User
                {
                    UserID = userId,
                    Username = currentUser.Username,
                    FullName = editModel.FullName,
                    Email = currentUser.Email,
                    Phone = editModel.Phone,
                    Address = editModel.Address,
                    Gender = editModel.Gender,
                    ImageUrl = imageUrl,
                    CreateAt = currentUser.CreateAt,
                    IsDelete = currentUser.IsDelete,
                    Role = currentUser.Role
                };
                HttpContext.Session.SetString("customerInfo", JsonConvert.SerializeObject(updatedUser));
                HttpContext.Session.SetString("Username", updatedUser.Username ?? updatedUser.FullName);
                return RedirectToAction("Profile");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Error: {errorContent}");
            ModelState.AddModelError("", $"Failed to update profile: {errorContent}");
            ViewBag.CurrentUser = currentUser;
            return View(editModel);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập
            var redirectResult = CheckAdminAccess();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Logic hiển thị danh sách người dùng (dành cho admin)
            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.GetAsync($"{_apiBaseUrl}/non-admin");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Unable to fetch user list.";
                    return View(new List<User>());
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(jsonString);
                if (users == null)
                {
                    ViewBag.ErrorMessage = "No users found.";
                    return View(new List<User>());
                }

                return View(users);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View(new List<User>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(int id)
        {
            _logger.LogInformation("Starting Block action for UserID: {UserID}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid UserID: {UserID} provided for block action.", id);
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction(nameof(Index));
            }

            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.PutAsync($"{_apiBaseUrl}/{id}/block", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("UserID: {UserID} blocked successfully.", id);
                    TempData["SuccessMessage"] = "User has been blocked successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to block UserID: {UserID}. Status: {StatusCode}. Error: {Error}", id, response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = $"Failed to block user: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while blocking UserID: {UserID}", id);
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}