using BookStore_Client.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BookStore_Client.Controllers
{
    public class SupplierController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://localhost:7202/api/Supplier";

        public SupplierController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Supplier
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            if (!response.IsSuccessStatusCode) return View(new List<Supplier>());
            var data = await response.Content.ReadAsStringAsync();
            var suppliers = JsonSerializer.Deserialize<List<Supplier>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(suppliers);
        }

        // GET: Supplier/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var data = await response.Content.ReadAsStringAsync();
            var supplier = JsonSerializer.Deserialize<Supplier>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(supplier);
        }

        // GET: Supplier/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (!ModelState.IsValid) return View(supplier);
            var jsonContent = new StringContent(JsonSerializer.Serialize(supplier), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrl, jsonContent);
            if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
            return View(supplier);
        }

        // GET: Supplier/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var data = await response.Content.ReadAsStringAsync();
            var supplier = JsonSerializer.Deserialize<Supplier>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(supplier);
        }

        // POST: Supplier/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.SupplierID) return BadRequest();
            if (!ModelState.IsValid) return View(supplier);
            var jsonContent = new StringContent(JsonSerializer.Serialize(supplier), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiUrl}/{id}", jsonContent);
            if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
            return View(supplier);
        }

        // GET: Supplier/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();
            var data = await response.Content.ReadAsStringAsync();
            var supplier = JsonSerializer.Deserialize<Supplier>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(supplier);
        }

        // POST: Supplier/Delete/{id}
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
