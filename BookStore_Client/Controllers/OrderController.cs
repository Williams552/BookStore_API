using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using BookStore_Client.Models;
using Microsoft.Extensions.Logging;
using BookStore_Client.Models.ViewModel;
using System.Text.Json;
using System.Text;
using BookStore_Client.Domain.DTO;

namespace BookStore_Client.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7218/api/OrderService/order";
        private readonly string _userApiUrl = "https://localhost:7202/api/user"; // Giả sử endpoint API cho User
        private readonly string _cartApiUrl = "https://localhost:7202/api/cart"; // Giả sử endpoint API cho Book
        private readonly ILogger<OrderController> _logger;

        public OrderController(HttpClient httpClient, ILogger<OrderController> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // GET: Order/Index
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Starting Index action to fetch orders.");

            try
            {
                _logger.LogDebug("Sending GET request to API: {ApiUrl}", _apiUrl);
                var response = await _httpClient.GetAsync(_apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API request failed with status code: {StatusCode}. Returning empty order list.", response.StatusCode);
                    return View(new List<Models.ViewModel.OrderViewModel>());
                }

                var data = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API response received: {Data}", data);

                if (string.IsNullOrEmpty(data))
                {
                    _logger.LogWarning("API returned empty data. Returning empty order list.");
                    return View(new List<Models.ViewModel.OrderViewModel>());
                }

                var orders = JsonConvert.DeserializeObject<List<Order>>(data);
                if (orders == null)
                {
                    _logger.LogError("Deserialization failed: orders is null. Returning empty order list.");
                    return View(new List<Models.ViewModel.OrderViewModel>());
                }

                var orderViewModels = new List<Models.ViewModel.OrderViewModel>();
                foreach (var order in orders)
                {
                    string userFullName = "Unknown";
                    if (order.UserID.HasValue)
                    {
                        _logger.LogDebug("Fetching user info for UserID: {UserID}", order.UserID);
                        var userResponse = await _httpClient.GetAsync($"{_userApiUrl}/{order.UserID}");
                        if (userResponse.IsSuccessStatusCode)
                        {
                            var userData = await userResponse.Content.ReadAsStringAsync();
                            _logger.LogDebug("User API response: {UserData}", userData); // Ghi log dữ liệu trả về
                            var user = JsonConvert.DeserializeObject<User>(userData);
                            userFullName = user?.FullName ?? "Unknown";
                            _logger.LogDebug("User FullName fetched: {FullName}", userFullName);
                        }
                        else
                        {
                            var errorContent = await userResponse.Content.ReadAsStringAsync();
                            _logger.LogWarning("Failed to fetch user info for UserID: {UserID}. Status: {StatusCode}. Error: {Error}", order.UserID, userResponse.StatusCode, errorContent);
                        }
                    }

                    orderViewModels.Add(new Models.ViewModel.OrderViewModel
                    {
                        OrderID = order.OrderID,
                        UserFullName = userFullName,
                        OrderDate = order.OrderDate,
                        TotalAmount = order.TotalAmount,
                        PaymentMethod = order.PaymentMethod,
                        Status = order.Status
                    });
                }

                _logger.LogInformation("Successfully fetched {OrderCount} orders.", orderViewModels.Count);
                return View(orderViewModels);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while calling API: {ApiUrl}", _apiUrl);
                return View(new List<Models.ViewModel.OrderViewModel>());
            }
            catch (System.Text.Json.JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize API response: {Data}", await _httpClient.GetStringAsync(_apiUrl));
                return View(new List<Models.ViewModel.OrderViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Index action.");
                return View(new List<Models.ViewModel.OrderViewModel>());
            }
        }

        // GET: Order/Cart (Giữ nguyên action hiện tại)
        public IActionResult Cart()
        {
            return View();
        }

        // POST: Order/ConfirmOrder/{id} (Xác nhận đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            try
            {
                var response = await _httpClient.PutAsync($"{_apiUrl}/{id}/confirm", null);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Order confirmed successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = $"Failed to confirm order: {response.StatusCode}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> HistoryOrder()
        {
            // Lấy userId từ session (hoặc token, tùy cách bạn lưu thông tin user)
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "User");
            }





            var response = await _httpClient.GetAsync($"user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<Models.ViewModel.OrderViewModel>>(content);
                return View(orders);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "User");
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to load order history.";
                return View(new List<Models.ViewModel.OrderViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(int paymentMethod, string address)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Lấy thông tin giỏ hàng từ API
            List<Cart> cartItems;
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_cartApiUrl}/getCartByUserId{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { success = false, message = "Không thể lấy thông tin giỏ hàng." });
                }
                var jsonString = await response.Content.ReadAsStringAsync();
                cartItems = System.Text.Json.JsonSerializer.Deserialize<List<Cart>>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (cartItems == null || cartItems.Count == 0)
                {
                    return Json(new { success = false, message = "Giỏ hàng trống." });
                }
            }

            // Tạo OrderDTO
            var orderDTO = new OrderDTO
            {
                UserID = userId.Value,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                TotalAmount = cartItems.Sum(c => c.TotalPrice ?? 0),
                PaymentMethod = paymentMethod,
                Status = "Pending",
                Address = paymentMethod == 1 ? address : null, // Chỉ lưu địa chỉ khi COD
                OrderDetails = cartItems.Select(c => new OrderDetailDTO
                {
                    BookID = c.BookID.Value,
                    Quantity = c.Quantity.Value
                }).ToList()
            };

            // Gửi yêu cầu tạo Order đến Order API
            var orderApiUrl = "https://localhost:7218/api/OrderService/Order";
            var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(orderDTO), Encoding.UTF8, "application/json");
            var orderResponse = await _httpClient.PostAsync(orderApiUrl, jsonContent);

            if (!orderResponse.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "Tạo đơn hàng thất bại." });
            }

            // Xóa giỏ hàng sau khi đặt hàng thành công
            var deleteCartResponse = await _httpClient.DeleteAsync($"{_cartApiUrl}/DeleteCartByUser/{userId}");
            if (!deleteCartResponse.IsSuccessStatusCode)
            {
                // Xử lý lỗi nếu cần
            }

            return Json(new { success = true, message = "Đặt hàng thành công!" });
        }
    }
}