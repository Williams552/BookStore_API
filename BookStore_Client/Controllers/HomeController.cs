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
            _httpClient.BaseAddress = new Uri("http://localhost:5242/");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var booksResponse = await _httpClient.GetAsync("api/book");
                if (!booksResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Không thể lấy danh sách sách từ API.");
                    return View(new HomeViewModel { Books = new List<Book>(), Categories = new List<Category>() });
                }

                var booksContent = await booksResponse.Content.ReadAsStringAsync();
                var books = JsonSerializer.Deserialize<List<Book>>(booksContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var categoriesResponse = await _httpClient.GetAsync("api/category");
                if (!categoriesResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Không thể lấy danh sách danh mục từ API.");
                    return View(new HomeViewModel { Books = books ?? new List<Book>(), Categories = new List<Category>() });
                }

                var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<Category>>(categoriesContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                // Lấy thông tin sách hiện tại
                var bookResponse = await _httpClient.GetAsync($"api/book/{id}");
                if (!bookResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Không thể lấy thông tin sách với ID {id} từ API.");
                    return NotFound();
                }

                var bookContent = await bookResponse.Content.ReadAsStringAsync();
                var book = JsonSerializer.Deserialize<Book>(bookContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (book == null)
                {
                    return NotFound();
                }

                // Lấy danh sách sách liên quan (cùng Author)
                var relatedBooksResponse = await _httpClient.GetAsync($"api/book?authorId={book.AuthorID}");
                if (!relatedBooksResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Không thể lấy danh sách sách liên quan với AuthorID {book.AuthorID} từ API.");
                    return NotFound();
                }

                var relatedBooksContent = await relatedBooksResponse.Content.ReadAsStringAsync();
                var relatedBooks = JsonSerializer.Deserialize<List<Book>>(relatedBooksContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Loại bỏ sách hiện tại khỏi danh sách sách liên quan
                relatedBooks = relatedBooks?.Where(b => b.BookID != book.BookID).ToList();

                // Tạo ViewModel
                var viewModel = new DetailViewModel
                {
                    Book = book,
                    RelatedBooks = relatedBooks ?? new List<Book>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin chi tiết sách với ID {id}.");
                return NotFound();
            }
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
