using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Users_API.Domain.DTO;
using Users_API.Models;
using Users_API.Repository;

namespace Users_API.Controllers
{
    [Route("api/UserService/{userId}/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IRepository<Cart> _cartRepository;

        public CartController(IRepository<Cart> cartRepository, IHttpClientFactory httpClientFactory)
        {
            _cartRepository = cartRepository;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7131/api/BookService/Book/");
        }

        // GET: api/users/{userId}/cart
        [HttpGet]
        public async Task<ActionResult> GetBooksInUserCart(int userId)
        {
            var cartItems = await _cartRepository.GetByCondition(c => c.UserID == userId);
            var bookIds = cartItems.Select(c => c.BookID).ToList();

            if (bookIds == null || !bookIds.Any())
            {
                return NotFound("No books found in user cart.");
            }

            // Get the details of each book from the Book_API
            var response = await _httpClient.PostAsJsonAsync("list", bookIds);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching book details.");
            }

            var books = await response.Content.ReadFromJsonAsync<List<BookDTO>>();
            return Ok(books);
        }

        // POST: api/users/{userId}/cart
        [HttpPost]
        public async Task<ActionResult> AddBookToCart(int userId, int bookId)
        {
            var cartItem = new Cart
            {
                UserID = userId,
                BookID = bookId,
                Quantity = 1
            };

            await _cartRepository.Add(cartItem);
            return Ok("Book added to cart.");
        }

        // DELETE: api/users/{userId}/cart/{bookId}
        [HttpDelete("{bookId}")]
        public async Task<ActionResult> RemoveBookFromCart(int userId, int bookId)
        {
            var cartItems = await _cartRepository.GetByCondition(c => c.UserID == userId && c.BookID == bookId);
            var cartItem = cartItems.FirstOrDefault();

            if (cartItem == null)
            {
                return NotFound("Book not found in cart.");
            }

            await _cartRepository.Delete(cartItem);
            return Ok("Book removed from cart.");
        }
    }
}
