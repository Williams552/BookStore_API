using BookStore_Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;

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
            var jsonData = System.Text.Json.JsonSerializer.Serialize(requestData);
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
                var error = System.Text.Json.JsonSerializer.Deserialize<dynamic>(errorContent);
                string errorMessage = error.Message?.ToString() ?? "An unknown error occurred from the API.";
                ModelState.AddModelError("Email", errorMessage);
                return BadRequest(ModelState);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                ModelState.AddModelError("Email", "Unable to connect to the API.");
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
                ModelState.AddModelError("", "Invalid session.");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(otp))
            {
                ModelState.AddModelError("", "OTP is required.");
                return BadRequest(ModelState);
            }

            // Kiểm tra thời gian OTP
            var otpSentTimeStr = HttpContext.Session.GetString("OtpSentTime");
            if (string.IsNullOrEmpty(otpSentTimeStr))
            {
                ModelState.AddModelError("", "OTP sent time not found.");
                return BadRequest(ModelState);
            }

            var otpSentTime = DateTime.Parse(otpSentTimeStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var timeElapsed = (DateTime.UtcNow - otpSentTime).TotalMinutes;
            if (timeElapsed > 1)
            {
                ModelState.AddModelError("", "OTP has expired (exceeded 2 minutes). Please request a new OTP.");
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("BookStoreAPI");
            var requestUrl = "api/User/verify-otp";
            var requestData = new { Email = email, Otp = otp };
            var jsonData = System.Text.Json.JsonSerializer.Serialize(requestData);
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
                var error = System.Text.Json.JsonSerializer.Deserialize<dynamic>(errorContent);
                string errorMessage = error.Message?.ToString() ?? "An unknown error occurred from the API.";
                ModelState.AddModelError("", errorMessage);
                return BadRequest(ModelState);
            }
            catch (HttpRequestException ex)
            {
                //Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                ModelState.AddModelError("", "Unable to connect to the API.");
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
                return BadRequest(new { Message = "Invalid session." });
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
                    return BadRequest(new { Message = "OTP is still valid. Please wait 2 minutes before requesting a new one." });
                }
            }

            var client = _httpClientFactory.CreateClient("BookStoreAPI");
            var requestUrl = "api/User/resend-otp";
            var requestData = new { Email = email };
            var jsonData = System.Text.Json.JsonSerializer.Serialize(requestData);
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
                    var result = System.Text.Json.JsonSerializer.Deserialize<dynamic>(content);
                    return Json(new { message = result.Message?.ToString() });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Response from API: {errorContent}");
                var error = System.Text.Json.JsonSerializer.Deserialize<dynamic>(errorContent);
                string errorMessage = error.Message?.ToString() ?? "An unknown error occurred from the API.";
                return BadRequest(new { Message = errorMessage });
            }
            catch (HttpRequestException ex)
            {
                //Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                return BadRequest(new { Message = "Unable to connect to the API." });
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
            var requestData = new { Email = model.Email, NewPassword = model.NewPassword, ConfirmPassword=model.ConfirmPassword };
            var jsonData = System.Text.Json.JsonSerializer.Serialize(requestData);
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
                var error = System.Text.Json.JsonSerializer.Deserialize<dynamic>(errorContent);
                string errorMessage = error.Message?.ToString() ?? "An unknown error occurred from the API.";
                ModelState.AddModelError("", errorMessage);
                return BadRequest(ModelState);
            }
            catch (HttpRequestException ex)
            {
                //Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                ModelState.AddModelError("", "Unable to connect to the API.");
                return BadRequest(ModelState);
            }
        }

        // Register
        [HttpGet("Register")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("BookStoreAPI");
            var requestUrl = "api/User/register";
            var requestData = new
            {
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                Phone = model.Phone
            };
            var jsonData = JsonConvert.SerializeObject(requestData);
            var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(requestUrl, requestContent);
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                    if (result.success == true)
                    {
                        // Tạo OTP và gửi email
                        Random random = new Random();
                        int otp = random.Next(100000, 999999);
                        await SendOTPEmailAsync(model.Email, otp.ToString());

                        // Lưu OTP vào API
                        var otpRequestUrl = "api/User/save-otp"; // Endpoint mới
                        var otpRequestData = new { Email = model.Email, Otp = otp };
                        var otpJsonData = JsonConvert.SerializeObject(otpRequestData);
                        var otpRequestContent = new StringContent(otpJsonData, Encoding.UTF8, "application/json");
                        var otpResponse = await client.PostAsync(otpRequestUrl, otpRequestContent);

                        if (otpResponse.IsSuccessStatusCode)
                        {
                            HttpContext.Session.SetString("Email", model.Email);
                            // Optional: Lưu OTP vào session nếu vẫn muốn kiểm tra local
                            HttpContext.Session.SetString("Otp", otp.ToString());
                            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString());
                            return RedirectToAction("ConfirmOtp");
                        }
                        else
                        {
                            var otpErrorContent = await otpResponse.Content.ReadAsStringAsync();
                            var otpErrorResult = JsonConvert.DeserializeObject<dynamic>(otpErrorContent);
                            ModelState.AddModelError("", otpErrorResult.Message?.ToString() ?? "Unable to save OTP.");
                            return View(model);
                        }
                    }
                    else
                    {
                        var errorMessage = result.message?.ToString() ?? "Registration failed.";
                        ModelState.AddModelError("", errorMessage);
                        return View(model);
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Response from API: {errorContent}");
                ModelState.AddModelError("", $"Registration failed.");
                return View(model);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error from call API: {ex.Message}");
                ModelState.AddModelError("", "Unable to connect to the API.");
                return View(model);
            }
        }

        // Thêm phương thức gửi email
        private async Task SendOTPEmailAsync(string email, string otp)
        {
            var fromAddress = new MailAddress("phamngocquan812@gmail.com", "Book Strore");
            var toAddress = new MailAddress(email);
            const string fromPassword = "ljzi zden qcwr mcwt"; // App Password
            const string subject = "Your OTP for Registration";
            string body = $"Dear {email},\n\nYour OTP code is {otp}. It is valid for 2 minutes.\n\nBest regards,\nCursus";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };
            await smtp.SendMailAsync(message);
        }

        [HttpGet("ConfirmOtp")]
        [AllowAnonymous]
        public IActionResult ConfirmOtp()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }
            ViewBag.Email = email;
            return View();
        }

        [HttpPost("ConfirmOtp")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmOtp(string email, string otp) // Bỏ User u
        {
            var sessionEmail = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(sessionEmail) || sessionEmail != email)
            {
                return Json(new { success = false, message = "Invalid session or email does not match." });
            }

            // Gửi yêu cầu đến API để xác nhận OTP và cập nhật IsActive
            var client = _httpClientFactory.CreateClient("BookStoreAPI");
            var requestUrl = "api/User/verify-otp";
            var requestData = new { Email = email, Otp = otp };
            var jsonData = JsonConvert.SerializeObject(requestData);
            var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(requestUrl, requestContent);
                Console.WriteLine($"Response Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.Remove("Otp");
                    HttpContext.Session.Remove("Email");
                    HttpContext.Session.Remove("OtpTime");
                    return Json(new { success = true, message = "Registration successful! Redirecting to the login page...", redirectUrl = "/api/Auth/User/login" });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response from API: {errorContent}");
                    var errorResult = JsonConvert.DeserializeObject<dynamic>(errorContent);
                    var errorMessage = errorResult?.Message?.ToString() ?? $"An unknown error occurred from the API (Status: {response.StatusCode})";
                    return Json(new { success = false, message = errorMessage });
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
                return Json(new { success = false, message = $"Unable to connect to the API {ex.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ConfirmOtp: {ex.Message}");
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}