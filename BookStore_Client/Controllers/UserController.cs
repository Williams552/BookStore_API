﻿using Microsoft.AspNetCore.Authentication.Cookies;
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

        public UserController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, HttpClient httpClient)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);
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

        // Thêm kiểm tra quyền truy cập cho các action khác trong UserController
        [HttpGet]
        public IActionResult Index()
        {
            var redirectResult = CheckAdminAccess();
            if (redirectResult != null)
            {
                return redirectResult;
            }

            // Logic hiển thị danh sách người dùng (dành cho admin)
            return View();
        }
    }
}