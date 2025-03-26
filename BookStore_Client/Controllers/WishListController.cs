using BookStore_Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;

namespace BookStore_Client.Controllers
{
    public class WishListController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7202/api/Wish";

        public WishListController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ShowNav = false;
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            List<WishList> cartItems = new List<WishList>();
            using (var client = _httpClient)
            {
                HttpResponseMessage response = await client.GetAsync($"{_apiUrl}/getByUserId/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    cartItems = System.Text.Json.JsonSerializer.Deserialize<List<WishList>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToList(int bookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            var requestUrl = $"{_apiUrl}/AddWish?bookId={bookId}&userId={userId}";
            var response = await _httpClient.PostAsync(requestUrl, new StringContent("", Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int bookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/?bookId={bookId}&userId={userId}");
            if (response.IsSuccessStatusCode) return RedirectToAction("Index", "WishList");
            return RedirectToAction("Index", "WishList");
        }
    }
}
