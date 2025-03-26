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

using System.Text;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using BookStore_Client.Domain.DTO;
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
                    return RedirectToAction("Index", "Home");
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

                    if (user.Role == 1)
                    {
                        return RedirectToAction("Index", "Users");
                    }
                    else if (user.Role == 3)
                    {
                        return RedirectToAction("Index", "Home");
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

        // GET: User/Index
        public async Task<IActionResult> Index()
        {
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

        // POST: User/Block/{id}
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