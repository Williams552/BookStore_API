using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using BookStore_Client.Models;
using Microsoft.Extensions.Logging;
using BookStore_Client.Models.ViewModel;

namespace BookStore_Client.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://localhost:7218/api/OrderService/order";
        private readonly string _userApiUrl = "http://localhost:7202/api/user"; // Giả sử endpoint API cho User
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
                    return View(new List<Models.OrderViewModel>());
                }

                var data = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API response received: {Data}", data);

                if (string.IsNullOrEmpty(data))
                {
                    _logger.LogWarning("API returned empty data. Returning empty order list.");
                    return View(new List<Models.OrderViewModel>());
                }

                var orders = JsonConvert.DeserializeObject<List<Order>>(data);
                if (orders == null)
                {
                    _logger.LogError("Deserialization failed: orders is null. Returning empty order list.");
                    return View(new List<Models.OrderViewModel>());
                }

                // Tạo danh sách OrderViewModel và lấy FullName từ API User
                var orderViewModels = new List<Models.OrderViewModel>();
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
                            var user = JsonConvert.DeserializeObject<User>(userData);
                            userFullName = user?.FullName ?? "Unknown";
                            _logger.LogDebug("User FullName fetched: {FullName}", userFullName);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to fetch user info for UserID: {UserID}. Status: {StatusCode}", order.UserID, userResponse.StatusCode);
                        }
                    }

                    orderViewModels.Add(new Models.OrderViewModel
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
                return View(new List<Models.OrderViewModel>());
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize API response: {Data}", await _httpClient.GetStringAsync(_apiUrl));
                return View(new List<Models.OrderViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Index action.");
                return View(new List<Models.OrderViewModel>());
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
                var orders = JsonConvert.DeserializeObject<List<Models.OrderViewModel>>(content);
                return View(orders);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "User");
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to load order history.";
                return View(new List<Models.OrderViewModel>());
            }
        }

    }
}