using BookStore_Client.Domain.DTO;
using BookStore_Client.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace BookStore_Client.Controllers
{
    public class AuthorController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "";

        public AuthorController(HttpClient httpClient)
        {
            _apiBaseUrl = "http://localhost:7202/api/author";
            _httpClient = httpClient;
        }

        // GET: Author
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_apiBaseUrl);
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<Author>());
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var authors = JsonConvert.DeserializeObject<List<Author>>(jsonResponse);
            return View(authors);
        }

        // GET: Author/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var author = JsonConvert.DeserializeObject<Author>(jsonResponse);
            return View(author);
        }

        // GET: Author/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Author/Create
        [HttpPost]
        public async Task<IActionResult> Create(Author author)
        {
            var authorDTO = new AuthorDTO
            {
                AuthorID = author.AuthorID,
                FullName = author.FullName,
                Biography = author.Biography,
                ImageURL = author.ImageURL
            };

            var jsonContent = JsonConvert.SerializeObject(authorDTO);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiBaseUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                return View(author);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Author/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var authorDTO = JsonConvert.DeserializeObject<AuthorDTO>(jsonResponse);

            var author = new Author
            {
                AuthorID = authorDTO.AuthorID,
                FullName = authorDTO.FullName,
                Biography = authorDTO.Biography,
                ImageURL = authorDTO.ImageURL
            };

            return View(author);
        }

        // POST: Author/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.AuthorID)
            {
                return BadRequest("Author ID mismatch.");
            }

            var authorDTO = new AuthorDTO
            {
                AuthorID = author.AuthorID,
                FullName = author.FullName,
                Biography = author.Biography,
                ImageURL = author.ImageURL
            };

            var jsonContent = JsonConvert.SerializeObject(authorDTO);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiBaseUrl}/{id}", content);
            if (!response.IsSuccessStatusCode)
            {
                return View(author);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Author/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var author = JsonConvert.DeserializeObject<Author>(jsonResponse);
            return View(author);
        }

        // POST: Author/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }
    }
}
