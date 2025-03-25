using BookStore_Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace BookStore_Client.Controllers
{
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("ForgotPassword")]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new password());
        }

        [HttpPost("ForgotPassword")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(password model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("BookStoreAPI");
            var requestUrl = "api/User/forgot-password";
            var requestData = new { Email = model.Email };
            var jsonData = JsonSerializer.Serialize(requestData);
            Console.WriteLine($"Request JSON sent to API: {jsonData}");
            Console.WriteLine($"Full Request URL: {client.BaseAddress}{requestUrl}");

            var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(requestUrl, requestContent);
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("Email", model.Email);
                    HttpContext.Session.SetString("OtpSentTime", DateTime.UtcNow.ToString("o")); // Lưu thời gian gửi OTP
                    return Json(new { redirectTo = Url.Action("VerifyOtpForPasswordReset") });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Response from API: {errorContent}");
                var error = JsonSerializer.Deserialize<dynamic>(errorContent);
                string errorMessage = error.Message?.ToString() ?? "Lỗi không xác định từ API";
                ModelState.AddModelError("Email", errorMessage);
                return BadRequest(ModelState);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                ModelState.AddModelError("Email", "Không thể kết nối đến API.");
                return BadRequest(ModelState);
            }
        }

        [HttpGet("VerifyOtpForPasswordReset")]
        [AllowAnonymous]
        public IActionResult VerifyOtpForPasswordReset()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = email;
            return View();
        }

        [HttpPost("VerifyOtpForPasswordReset")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtpForPasswordReset(string email, string otp)
        {
            var sessionEmail = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(sessionEmail) || sessionEmail != email)
            {
                ModelState.AddModelError("", "Phiên làm việc không hợp lệ.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(otp))
            {
                ModelState.AddModelError("", "OTP là bắt buộc.");
                return BadRequest(ModelState);
            }

            // Kiểm tra thời gian OTP
            var otpSentTimeStr = HttpContext.Session.GetString("OtpSentTime");
            if (string.IsNullOrEmpty(otpSentTimeStr))
            {
                ModelState.AddModelError("", "Không tìm thấy thời gian gửi OTP.");
                return BadRequest(ModelState);
            }

            var otpSentTime = DateTime.Parse(otpSentTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var timeElapsed = (DateTime.UtcNow - otpSentTime).TotalMinutes;
            if (timeElapsed > 1)
            {
                ModelState.AddModelError("", "OTP đã hết hạn (vượt quá 2 phút). Vui lòng yêu cầu gửi lại OTP.");
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("BookStoreAPI");
            var requestUrl = "api/User/verify-otp";
            var requestData = new { Email = email, Otp = otp };
            var jsonData = JsonSerializer.Serialize(requestData);
            Console.WriteLine($"Request JSON sent to API: {jsonData}");
            Console.WriteLine($"Full Request URL: {client.BaseAddress}{requestUrl}");

            var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(requestUrl, requestContent);
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.Remove("Email");
                    HttpContext.Session.Remove("OtpSentTime"); // Xóa session sau khi xác thực
                    return Json(new { redirectTo = Url.Action("ResetPassword", new { email }) });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Response from API: {errorContent}");
                var error = JsonSerializer.Deserialize<dynamic>(errorContent);
                string errorMessage = error.Message?.ToString() ?? "Lỗi không xác định từ API";
                ModelState.AddModelError("", errorMessage);
                return BadRequest(ModelState);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                ModelState.AddModelError("", "Không thể kết nối đến API.");
                return BadRequest(ModelState);
            }
        }

        [HttpPost("ResendOtp")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp(string email)
        {
            Console.WriteLine($"Received email: {email}");
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("Email is null or empty");
                return BadRequest(new { Message = "Email is required." });
            }

            var sessionEmail = HttpContext.Session.GetString("Email");
            Console.WriteLine($"Session email: {sessionEmail}");
            if (sessionEmail != email)
            {
                Console.WriteLine("Session email does not match received email");
                return BadRequest(new { Message = "Phiên làm việc không hợp lệ." });
            }

            // Kiểm tra thời gian OTP trong session
            var otpSentTimeStr = HttpContext.Session.GetString("OtpSentTime");
            if (!string.IsNullOrEmpty(otpSentTimeStr))
            {
                var otpSentTime = DateTime.Parse(otpSentTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
                var timeElapsed = (DateTime.UtcNow - otpSentTime).TotalMinutes;
                if (timeElapsed <= 2)
                {
                    Console.WriteLine("OTP still valid, less than 2 minutes elapsed");
                    return BadRequest(new { Message = "OTP hiện tại vẫn còn hiệu lực. Vui lòng đợi 2 phút trước khi yêu cầu gửi lại." });
                }
            }

            var client = _httpClientFactory.CreateClient("BookStoreAPI");
            var requestUrl = "api/User/resend-otp";
            var requestData = new { Email = email };
            var jsonData = JsonSerializer.Serialize(requestData);
            Console.WriteLine($"Request JSON sent to API: {jsonData}");
            Console.WriteLine($"Full Request URL: {client.BaseAddress}{requestUrl}");

            var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            Console.WriteLine($"Request Content Headers: {requestContent.Headers}");

            try
            {
                var response = await client.PostAsync(requestUrl, requestContent);
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("OtpSentTime", DateTime.UtcNow.ToString("o"));
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {content}");
                    var result = JsonSerializer.Deserialize<dynamic>(content);
                    return Json(new { message = result.Message?.ToString() });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Response from API: {errorContent}");
                var error = JsonSerializer.Deserialize<dynamic>(errorContent);
                string errorMessage = error.Message?.ToString() ?? "Lỗi không xác định từ API";
                return BadRequest(new { Message = errorMessage });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                return BadRequest(new { Message = "Không thể kết nối đến API." });
            }
        }

        [HttpGet("ResetPassword")]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }
            ViewBag.Email = email;
            return View(new password { Email = email });
        }

        [HttpPost("ResetPassword")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(password model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "New Password is required.");
                return BadRequest(ModelState);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match.");
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("BookStoreAPI");
            var requestUrl = "api/User/reset-password";
            var requestData = new { Email = model.Email, NewPassword = model.NewPassword, ConfirmPassword = model.ConfirmPassword };
            var jsonData = JsonSerializer.Serialize(requestData);
            Console.WriteLine($"Request JSON sent to API: {jsonData}");
            Console.WriteLine($"Full Request URL: {client.BaseAddress}{requestUrl}");

            var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(requestUrl, requestContent);
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { redirectTo = "/api/Auth/User/Login" });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Response from API: {errorContent}");
                var error = JsonSerializer.Deserialize<dynamic>(errorContent);
                string errorMessage = error.Message?.ToString() ?? "Lỗi không xác định từ API";
                ModelState.AddModelError("", errorMessage);
                return BadRequest(ModelState);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                ModelState.AddModelError("", "Không thể kết nối đến API.");
                return BadRequest(ModelState);
            }
        }
    }
}