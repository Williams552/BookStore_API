using BookStore_Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace BookStore_Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7202/"); // Thay đổi URL của API nếu cần
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Gọi API để lấy danh sách sách
                var booksResponse = await _httpClient.GetAsync("api/book");
                if (!booksResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Không thể lấy danh sách sách từ API.");
                    return View(new HomeViewModel { Books = new List<Book>(), Categories = new List<Category>() });
                }

                var booksContent = await booksResponse.Content.ReadAsStringAsync();
                var books = JsonSerializer.Deserialize<List<Book>>(booksContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Gọi API để lấy danh sách danh mục
                var categoriesResponse = await _httpClient.GetAsync("api/category");
                if (!categoriesResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Không thể lấy danh sách danh mục từ API.");
                    return View(new HomeViewModel { Books = books ?? new List<Book>(), Categories = new List<Category>() });
                }

                var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<Category>>(categoriesContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Tạo HomeViewModel và truyền vào view
                var model = new HomeViewModel
                {
                    Books = books ?? new List<Book>(),
                    Categories = categories ?? new List<Category>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu cho trang chủ.");
                return View(new HomeViewModel { Books = new List<Book>(), Categories = new List<Category>() });
            }
        }

        public IActionResult Detail()
        {
            return View();
        }

        public IActionResult Author()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
