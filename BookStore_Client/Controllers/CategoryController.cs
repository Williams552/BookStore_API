using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BookStore_Client.Models;
using BookStore_API.Domain.DTO;
using static System.Net.WebRequestMethods;
using BookStore_Client.Domain.DTO;

namespace BookStore_Client.Controllers
{
    public class CategoryController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "";

        public CategoryController()
        {
            _apiBaseUrl = "https://localhost:7202/api/Category";
            _httpClient = new HttpClient();
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_apiBaseUrl);
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<Category>());
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<List<Category>>(jsonResponse);
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var category = JsonConvert.DeserializeObject<Category>(jsonResponse);
            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        public async Task<IActionResult> Create(CategoryDTO categoryDTO)
        {
            var jsonContent = JsonConvert.SerializeObject(categoryDTO);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiBaseUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                return View(categoryDTO);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var categoryDTO = JsonConvert.DeserializeObject<CategoryDTO>(jsonResponse);

            // Chuyển đổi từ CategoryDTO sang Category để truyền vào View
            var category = new Category
            {
                CategoryID = categoryDTO.CategoryID,
                CategoryName = categoryDTO.CategoryName,
                Description = categoryDTO.Description
            };

            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryID)
            {
                return BadRequest("Category ID mismatch.");
            }

            var categoryDTO = new CategoryDTO
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                Description = category.Description
            };

            var jsonContent = JsonConvert.SerializeObject(categoryDTO);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{id}", content);
            if (!response.IsSuccessStatusCode)
            {
                return View(category);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var categoryDTO = JsonConvert.DeserializeObject<CategoryDTO>(jsonResponse);

            // Chuyển đổi từ CategoryDTO sang Category để truyền vào View
            var category = new Category
            {
                CategoryID = categoryDTO.CategoryID,
                CategoryName = categoryDTO.CategoryName,
                Description = categoryDTO.Description
            };

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to delete category.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
