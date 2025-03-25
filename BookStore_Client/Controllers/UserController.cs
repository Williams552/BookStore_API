﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using BookStore_Client.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using NuGet.Common;
using System.Net.Http.Headers;
using BookStore_API.Domain.DTO;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using BookStore_Client.Domain.DTO;
using BookStore_API.DataAccess;

namespace BookStore_Client.Controllers
{
    [Route("api/Auth/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly string _apiBaseUrl = "";
        private readonly BookStoreContext _context;
        private readonly HttpClient _httpClient = null;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, HttpClient httpClient, BookStoreContext context)
        {
            _apiBaseUrl = "https://localhost:7202/api/User";
            _context = context;
            _httpClient = new HttpClient();
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _httpClient.DefaultRequestHeaders.Accept.Add(contentType);
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
                var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetInt32("UserID", user.UserID);
                    var json = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng!");
                return View(user);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Tài khoản hoặc mật khẩu không đúng!");
                return View(user);
            }
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] User user)
        {
            var userDTO = new User
            {
                Username = user.Username,
                Password = user.Password,
                Email = user.Email,
                Phone = user.Phone
            };

            var jsonContent = JsonConvert.SerializeObject(userDTO);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/register", content);
            if (!response.IsSuccessStatusCode)
            {
                return View(user);
            }
            return RedirectToAction("Login", "User");
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
                    user = new BookStore_API.Models.User
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
                HttpContext.Session.SetString("Username", user.Username);
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

        

        

        //public async Task<IActionResult> Profile()
        //{
        //    // Lấy thông tin user từ session
        //    var userDataJson = HttpContext.Session.GetString("customerInfo");
        //    if (string.IsNullOrEmpty(userDataJson))
        //    {
        //        // Nếu chưa đăng nhập, chuyển hướng về trang login
        //        return RedirectToAction("Login", "User");
        //    }

        //    var user = JsonConvert.DeserializeObject<User>(userDataJson);

        //    // Gọi API để lấy thông tin user mới nhất từ DB
        //    var httpClient = _httpClientFactory.CreateClient();
        //    var apiUrl = $"https://localhost:7202/api/User/by-email/{user.Email}";
        //    var response = await httpClient.GetAsync(apiUrl);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var jsonResponse = await response.Content.ReadAsStringAsync();
        //        var updatedUser = JsonConvert.DeserializeObject<User>(jsonResponse);

        //        // Cập nhật lại session với thông tin mới nhất
        //        HttpContext.Session.SetString("customerInfo", JsonConvert.SerializeObject(updatedUser));
        //        return View(updatedUser); // Trả về view với thông tin user
        //    }

        //    // Nếu không lấy được từ API, dùng dữ liệu từ session
        //    return View(user);
        //}

        //public async Task<IActionResult> EditProfile(string email)
        //{
        //    // Lấy thông tin user từ session
        //    var userDataJson = HttpContext.Session.GetString("customerInfo");
        //    if (string.IsNullOrEmpty(userDataJson))
        //    {
        //        return RedirectToAction("Login", "User");
        //    }

        //    var user = JsonConvert.DeserializeObject<User>(userDataJson);

        //    // Kiểm tra email từ route có khớp với user trong session không
        //    if (user.Email != email)
        //    {
        //        return Unauthorized("You can only edit your own profile.");
        //    }

        //    // Gọi API để lấy thông tin mới nhất
        //    var httpClient = _httpClientFactory.CreateClient();
        //    var apiUrl = $"https://localhost:7202/api/User/by-email/{email}";
        //    var response = await httpClient.GetAsync(apiUrl);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var jsonResponse = await response.Content.ReadAsStringAsync();
        //        var updatedUser = JsonConvert.DeserializeObject<User>(jsonResponse);
        //        return View(updatedUser); // Trả về view với thông tin user
        //    }

        //    return View(user); // Dùng dữ liệu từ session nếu API không hoạt động
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditProfile(User updatedUser, IFormFile ImageFile)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        // Log các lỗi validation
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        //        foreach (var error in errors)
        //        {
        //            Console.WriteLine($"Validation Error: {error}");
        //        }
        //        return View(updatedUser); // Trả lại form nếu dữ liệu không hợp lệ
        //    }

        //    // Lấy thông tin user từ session để kiểm tra quyền
        //    var userDataJson = HttpContext.Session.GetString("customerInfo");
        //    if (string.IsNullOrEmpty(userDataJson))
        //    {
        //        return RedirectToAction("Login", "User");
        //    }

        //    var currentUser = JsonConvert.DeserializeObject<User>(userDataJson);
        //    if (currentUser.Email != updatedUser.Email)
        //    {
        //        return Unauthorized("You can only edit your own profile.");
        //    }

        //    // Xử lý upload ảnh nếu có
        //    string imageUrl = updatedUser.ImageUrl ?? string.Empty; // Đảm bảo không null
        //    if (ImageFile != null && ImageFile.Length > 0)
        //    {
        //        // Kiểm tra kích thước file (giới hạn 5MB)
        //        if (ImageFile.Length > 5 * 1024 * 1024)
        //        {
        //            ModelState.AddModelError("ImageFile", "File size must be less than 5MB.");
        //            return View(updatedUser);
        //        }

        //        // Kiểm tra loại file (chỉ cho phép ảnh)
        //        if (!ImageFile.ContentType.StartsWith("image/"))
        //        {
        //            ModelState.AddModelError("ImageFile", "Please upload a valid image file.");
        //            return View(updatedUser);
        //        }

        //        // Định nghĩa thư mục lưu ảnh
        //        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        //        if (!Directory.Exists(uploadsFolder))
        //        {
        //            Directory.CreateDirectory(uploadsFolder);
        //        }

        //        // Xóa ảnh cũ nếu có
        //        if (!string.IsNullOrEmpty(updatedUser.ImageUrl))
        //        {
        //            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", updatedUser.ImageUrl.TrimStart('/'));
        //            if (System.IO.File.Exists(oldFilePath))
        //            {
        //                System.IO.File.Delete(oldFilePath);
        //            }
        //        }

        //        // Tạo tên file duy nhất
        //        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
        //        var filePath = Path.Combine(uploadsFolder, fileName);

        //        // Lưu file vào thư mục
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await ImageFile.CopyToAsync(stream);
        //        }

        //        // Cập nhật ImageUrl
        //        imageUrl = $"/uploads/{fileName}";
        //    }

        //    // Gọi API để cập nhật thông tin
        //    var httpClient = _httpClientFactory.CreateClient();
        //    var apiUrl = $"https://localhost:7202/api/User/{currentUser.UserID}";
        //    var userDTO = new UserDTO
        //    {
        //        UserID = currentUser.UserID,
        //        Username = updatedUser.Username, // Đảm bảo Username được gửi
        //        FullName = updatedUser.FullName,
        //        Email = updatedUser.Email,
        //        Phone = updatedUser.Phone,
        //        Address = updatedUser.Address,
        //        Gender = updatedUser.Gender,
        //        ImageUrl = imageUrl // Cập nhật ImageUrl (mới hoặc giữ nguyên)
        //    };

        //    // Log dữ liệu gửi lên API
        //    Console.WriteLine($"Sending to API: {JsonConvert.SerializeObject(userDTO)}");

        //    var response = await httpClient.PutAsJsonAsync(apiUrl, userDTO);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        // Cập nhật lại session với thông tin mới
        //        updatedUser.ImageUrl = imageUrl; // Cập nhật ImageUrl vào updatedUser
        //        updatedUser.CreateAt = currentUser.CreateAt; // Giữ nguyên CreateAt
        //        updatedUser.IsDelete = currentUser.IsDelete; // Giữ nguyên IsDelete
        //        updatedUser.Role = currentUser.Role;         // Giữ nguyên Role
        //        var updatedUserJson = JsonConvert.SerializeObject(updatedUser);
        //        HttpContext.Session.SetString("customerInfo", updatedUserJson);
        //        HttpContext.Session.SetString("Username", updatedUser.Username ?? updatedUser.FullName);

        //        return RedirectToAction("Profile"); // Quay lại trang profile sau khi cập nhật
        //    }

        //    // Log lỗi từ API
        //    var errorContent = await response.Content.ReadAsStringAsync();
        //    ModelState.AddModelError("", $"Failed to update profile: {errorContent}");
        //    Console.WriteLine($"API Error: {errorContent}");
        //    return View(updatedUser);
        //}

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ session
            return RedirectToAction("Login", "User");
        }
    }
}
