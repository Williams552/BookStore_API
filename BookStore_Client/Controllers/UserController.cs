using BookStore_API.DataAccess;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using BookStore_API.Models;

namespace BookStore_Client.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BookStoreContext _context;

        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpContextAccessor httpContextAccessor, BookStoreContext context, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction(nameof(Login)); // Nếu thất bại, quay về trang đăng nhập
            }

            var claims = authenticateResult.Principal.Identities
                .FirstOrDefault()?.Claims.ToList();

            var emailClaim = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var firstName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "";
            var lastName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? "";

            if (!string.IsNullOrEmpty(emailClaim))
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == emailClaim);

                if (user == null)
                {
                    // Tạo tài khoản mới nếu chưa có trong DB
                    user = new User
                    {
                        Username = $"{firstName} {lastName}".Trim(),
                        Email = emailClaim,
                        Password = null, // Không cần mật khẩu vì dùng Google
                        FullName = $"{firstName} {lastName}".Trim(), // Ghép tên thành FullName
                        Gender = null, // Google không trả về giới tính
                        Address = null,
                        Phone = null,
                        ImageUrl = null, // Có thể lấy ảnh từ Google sau này nếu cần
                        CreateAt = DateOnly.FromDateTime(DateTime.Now),
                        UpdateAt = null,
                        IsDelete = false,
                        Role = 3 // Giả sử 3 là role của user thường
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Lưu thông tin vào session
                HttpContext.Session.Remove("customerInfo");
                string userDataJson = JsonConvert.SerializeObject(user);
                HttpContext.Session.SetString("customerInfo", userDataJson);

                // Điều hướng dựa trên Role
                if (user.Role == 1)
                {
                    return RedirectToAction("Index", "Users");
                }
                else if (user.Role == 3)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // Nếu không có email, quay về trang login
            return RedirectToAction(nameof(Login));
        }

        private string GenerateOTP()
        {
            Random random = new Random();
            int otpNumber = random.Next(100000, 999999);
            return otpNumber.ToString();
        }

        private async Task SendOTPEmailAsync(string email, string otp)
        {
            var fromAddress = new MailAddress("phamngocquan812@gmail.com", "Cursus");
            var toAddress = new MailAddress(email);
            const string fromPassword = "ljzi zden qcwr mcwt";
            const string subject = "Your OTP Code";
            string body = $"Dear, {email}\n\nYour OTP code is {otp}\n\nBest regards,\nCursus";
            HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString());
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
            smtp.Send(message);
        }

        [HttpGet]
        public IActionResult ConfirmOtp()
        {
            var email = HttpContext.Session.GetString("Email");
            ViewBag.Email = email;
            return View();
        }

        // POST: Register/ConfirmOtp
        [HttpPost]
        public async Task<IActionResult> ConfirmOtp(string email, string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("Otp");

            if (sessionOtp == otp)
            {
                var otpTimeStr = HttpContext.Session.GetString("OtpTime");
                if (!string.IsNullOrEmpty(otpTimeStr))
                {
                    DateTime otpTime = DateTime.Parse(otpTimeStr);
                    DateTime otpExpirationTime = otpTime.AddMinutes(2); // OTP hết hạn sau 2 phút

                    if (DateTime.Now > otpExpirationTime)
                    {
                        HttpContext.Session.Remove("Otp");
                        HttpContext.Session.Remove("Email");
                        HttpContext.Session.Remove("OtpTime");

                        return Json(new { success = false, message = "OTP đã hết hạn. Vui lòng gửi lại mã OTP mới." });
                    }
                }
                else
                {
                    HttpContext.Session.Remove("Otp");
                    HttpContext.Session.Remove("Email");
                    HttpContext.Session.Remove("OtpTime");

                    return Json(new { success = false, message = "Không tìm thấy thông tin OTP trước đó." });
                }

                // Gọi API lấy thông tin user theo email
                var httpClient = _httpClientFactory.CreateClient();
                var apiUrl = $"https://localhost:7202/api/User/by-email/{email}";  // API của bạn
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<User>(jsonResponse);

                    if (user != null)
                    {
                        user.Role = 1; // Cập nhật trạng thái active

                        // Cập nhật trạng thái user qua API
                        var updateResponse = await httpClient.PutAsJsonAsync("https://localhost:7202/api/User/updateStatus", user);

                        if (updateResponse.IsSuccessStatusCode)
                        {
                            HttpContext.Session.Remove("Otp");
                            HttpContext.Session.Remove("Email");
                            HttpContext.Session.Remove("OtpTime");

                            if (user.Role == 3)
                            {
                                return Json(new { success = true, message = "Đăng ký thành công! Chuyển hướng đến trang chờ phê duyệt...", redirectUrl = Url.Action("WaitApprove", "Home") });
                            }
                            else
                            {
                                return Json(new { success = true, message = "Đăng ký thành công! Chuyển hướng đến trang đăng nhập...", redirectUrl = Url.Action("Login", "Home") });
                            }
                        }
                    }
                }

                return Json(new { success = false, message = "Không tìm thấy người dùng với email này." });
            }

            return Json(new { success = false, message = "Mã OTP không đúng." });
        }



        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://localhost:7202/api/User/by-email/{email}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(jsonResponse);

                if (user == null)
                {
                    return Json(new { success = false, message = "Email does not exist." });
                }

                // Tạo OTP mới
                string otp = GenerateOTP();

                // Gửi OTP mới qua email
                await SendOTPEmailAsync(email, otp);

                // Lưu OTP mới vào session
                HttpContext.Session.SetString("Otp", otp);
                HttpContext.Session.SetString("OtpTime", DateTime.Now.ToString());

                return Json(new { success = true, message = "New OTP code has been sent." });
            }

            return Json(new { success = false, message = "Failed to fetch user data from API." });
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Register/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email không tồn tại trong hệ thống.");
                return View();
            }

            string otp = GenerateOTP();
            await SendOTPEmailAsync(email, otp);

            HttpContext.Session.SetString("Otp", otp);
            HttpContext.Session.SetString("Email", user.Email);
            Console.WriteLine($"Saved to session - Email: '{user.Email}', OTP: '{otp}'");

            return RedirectToAction("VerifyOtpForPasswordReset");
        }


        public IActionResult VerifyOtpForPasswordReset()
        {
            var email = HttpContext.Session.GetString("Email");
            ViewBag.Email = email;
            return View();
        }

        // POST: Register/VerifyOtpForPasswordReset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtpForPasswordReset(string email, string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("Otp")?.Trim();
            var sessionEmail = HttpContext.Session.GetString("Email")?.Trim();
            otp = otp?.Trim();

            Console.WriteLine($"Session Email: '{sessionEmail}', Form Email: '{email}', Session OTP: '{sessionOtp}', Input OTP: '{otp}'");

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(sessionEmail))
            {
                ModelState.AddModelError("Otp", "Phiên OTP không hợp lệ. Vui lòng thử lại từ bước quên mật khẩu.");
                ViewBag.Email = sessionEmail ?? email; // Gán lại ViewBag.Email
                return View();
            }

            if (!string.Equals(sessionEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Otp", "Email không khớp với phiên hiện tại.");
                ViewBag.Email = sessionEmail; // Gán lại ViewBag.Email
                return View();
            }

            if (string.Equals(sessionOtp, otp))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == sessionEmail);
                if (user == null)
                {
                    ModelState.AddModelError("Otp", "Không tìm thấy người dùng với email này.");
                    ViewBag.Email = sessionEmail; // Gán lại ViewBag.Email
                    return View();
                }

                HttpContext.Session.Remove("Otp");
                HttpContext.Session.Remove("Email");
                return RedirectToAction("ResetPassword", new { email = user.Email });
            }

            ModelState.AddModelError("Otp", "OTP không chính xác.");
            ViewBag.Email = sessionEmail; // Gán lại ViewBag.Email
            return View();
        }

        // GET: Register/ResetPassword
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // POST: Register/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string newPassword, string confirmPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            // Hash the new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Redirect("/User/Login");


        }
    }
}
