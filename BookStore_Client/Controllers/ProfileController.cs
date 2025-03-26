using BookStore_Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BookStore_Client.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "https://localhost:7202/api/User";

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _apiBaseUrl = "https://localhost:7202/api/User";
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:7202/api/User/"); // Thay bằng URL API thực tế
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 1; // Lấy từ session thay vì cố định
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
                }
                else
                {
                    ViewBag.ErrorMessage = "Unable to fetch profile data.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View();
            }
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
}