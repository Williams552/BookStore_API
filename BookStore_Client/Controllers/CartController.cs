using Microsoft.AspNetCore.Mvc;
using BookStore_Client.Models;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace BookStore_Client.Controllers
{
    public class CartController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7202/api/Cart";

        public CartController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Cart()
        {
            ViewBag.ShowNav = false;
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            List<Cart> cartItems = new List<Cart>();
            using (var client = _httpClient)
            {
                int cartItemCount = 0;
                HttpResponseMessage response = await client.GetAsync($"{_apiUrl}/getCartByUserId/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    cartItems = System.Text.Json.JsonSerializer.Deserialize<List<Cart>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    cartItemCount = cartItems.Sum(c => c.Quantity ?? 0);
                    HttpContext.Session.SetInt32("CartItemCount", cartItemCount);
                }
            }
            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            var requestUrl = $"{_apiUrl}/AddToCart?bookId={bookId}&userId={userId}&quantity={quantity}";
            var response = await _httpClient.PostAsync(requestUrl, new StringContent("", Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "Thêm thất bại!" }, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            }
            return Json(new { success = true, message = "Thêm thành công!" }, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }


        [HttpPost]
        public async Task<IActionResult> Upsert(int bookId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });
            }
            var requestUrl = $"{_apiUrl}/Upsert?bookId={bookId}&userId={userId}&quantity={quantity}";
            var response = await _httpClient.PostAsync(requestUrl, new StringContent("", Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "Số lượng sách còn lại không đủ!" });
            }
            return Json(new { success = true, message = "Thêm sản phẩm thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int bookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/?bookId={bookId}&userId={userId}");
            if (response.IsSuccessStatusCode) return RedirectToAction("Cart", "Cart");
            return RedirectToAction("Cart", "Cart");
        }

    }
}
