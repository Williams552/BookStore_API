using BookStore_API.Domain.DTO;
using BookStore_Client.Models;
using BookStore_Client.Models.ViewModels;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookStore_Client.Controllers
{
    public class BookController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7202/api/book";

        public BookController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Book
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            if (!response.IsSuccessStatusCode) return View(new List<Book>());

            var data = await response.Content.ReadAsStringAsync();
            var books = JsonConvert.DeserializeObject<List<Book>>(data);
            return View(books);
        }

        // GET: Book/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var book = JsonConvert.DeserializeObject<Book>(data);

            if (book == null) return NotFound();

            // Gọi API lấy Author, Category, Supplier
            var authorResponse = await _httpClient.GetAsync($"https://localhost:7202/api/author/{book.AuthorID}");
            var categoryResponse = await _httpClient.GetAsync($"https://localhost:7202/api/category/{book.CategoryID}");
            var supplierResponse = await _httpClient.GetAsync($"https://localhost:7202/api/supplier/{book.SupplierID}");

            var author = authorResponse.IsSuccessStatusCode ? JsonConvert.DeserializeObject<Author>(await authorResponse.Content.ReadAsStringAsync()) : null;
            var category = categoryResponse.IsSuccessStatusCode ? JsonConvert.DeserializeObject<Category>(await categoryResponse.Content.ReadAsStringAsync()) : null;
            var supplier = supplierResponse.IsSuccessStatusCode ? JsonConvert.DeserializeObject<Supplier>(await supplierResponse.Content.ReadAsStringAsync()) : null;

            var bookViewModel = new BookViewModel
            {
                BookID = book.BookID,
                Title = book.Title,
                Description = book.Description,
                Price = book.Price ?? 0,
                Stock = book.Stock ?? 0,
                PublicDate = book.PublicDate?.ToDateTime(TimeOnly.MinValue),
                ImageURL = book.ImageURL,
                UpdateAt = book.UpdateAt?.ToDateTime(TimeOnly.MinValue),
                UpdateBy = book.UpdateBy ?? 0,
                AuthorName = author?.FullName ?? "Unknown",
                CategoryName = category?.CategoryName ?? "Unknown",
                SupplierName = supplier?.SupplierName ?? "Unknown"
            };

            return View(bookViewModel);
        }


        // GET: Book/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (!ModelState.IsValid) return View(book);

            var jsonContent = new StringContent(JsonConvert.SerializeObject(book), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrl, jsonContent);

            if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
            return View(book);
        }

        // GET: Book/Edit/{id}
        // GET: Book/Edit/{id}
        // GET: Book/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var book = JsonConvert.DeserializeObject<Book>(data);

            if (book == null) return NotFound();

            var bookViewModel = new BookViewModel
            {
                BookID = book.BookID,
                Title = book.Title,
                Description = book.Description,
                Price = book.Price ?? 0,
                Stock = book.Stock ?? 0,
                PublicDate = book.PublicDate?.ToDateTime(TimeOnly.MinValue),
                ImageURL = book.ImageURL,
                UpdateBy = book.UpdateBy,
                UpdateAt = book.UpdateAt?.ToDateTime(TimeOnly.MinValue),
                AuthorID = book.AuthorID ?? 0,
                CategoryID = book.CategoryID ?? 0,
                SupplierID = book.SupplierID ?? 0,
                Authors = await GetAuthors(),
                Categories = await GetCategories(),
                Suppliers = await GetSuppliers()
            };

            return View(bookViewModel);
        }

        // POST: Book/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel bookViewModel)
        {
            if (id != bookViewModel.BookID) return BadRequest();

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Error: {error.ErrorMessage}");
                }
                bookViewModel.Authors = await GetAuthors();
                bookViewModel.Categories = await GetCategories();
                bookViewModel.Suppliers = await GetSuppliers();
                return View(bookViewModel);
            }


            // Cập nhật thông tin
            var book = new Book
            {
                BookID = bookViewModel.BookID,
                Title = bookViewModel.Title,
                Description = bookViewModel.Description,
                Price = bookViewModel.Price,
                Stock = bookViewModel.Stock,
                PublicDate = bookViewModel.PublicDate.HasValue ? (DateOnly?)DateOnly.FromDateTime(bookViewModel.PublicDate.Value) : null,
                ImageURL = bookViewModel.ImageURL,
                UpdateBy = 1,
                UpdateAt = DateOnly.FromDateTime(DateTime.Now),
                AuthorID = bookViewModel.AuthorID,
                CategoryID = bookViewModel.CategoryID,
                SupplierID = bookViewModel.SupplierID
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(book), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiUrl}/{id}", jsonContent);


            if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            bookViewModel.Authors = await GetAuthors();
            bookViewModel.Categories = await GetCategories();
            bookViewModel.Suppliers = await GetSuppliers();

            return View(bookViewModel);
        }




        // Hàm gọi API lấy danh sách Authors, Categories, Suppliers
        private async Task<List<Author>> GetAuthors()
        {
            var response = await _httpClient.GetAsync("https://localhost:7202/api/Author");
            return response.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<Author>>(await response.Content.ReadAsStringAsync())
                : new List<Author>();
        }

        private async Task<List<Category>> GetCategories()
        {
            var response = await _httpClient.GetAsync("https://localhost:7202/api/Category");
            return response.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<Category>>(await response.Content.ReadAsStringAsync())
                : new List<Category>();
        }

        private async Task<List<Supplier>> GetSuppliers()
        {
            var response = await _httpClient.GetAsync("https://localhost:7202/api/Supplier");
            return response.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<Supplier>>(await response.Content.ReadAsStringAsync())
                : new List<Supplier>();
        }


        // GET: Book/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var book = JsonConvert.DeserializeObject<Book>(data);
            return View(book);
        }

        // POST: Book/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
            if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
            return NotFound();
        }
    }
}
