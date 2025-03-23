using BookStore_Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BookStore_Client.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7202/api/User/"); // Thay bằng URL API thực tế

            try
            {
                int userId = 1; // UserID cố định
                var response = await client.GetAsync($"https://localhost:7202/api/User/{userId}");

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
    }
}
