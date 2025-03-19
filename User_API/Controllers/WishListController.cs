using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Users_API.Domain.DTO;
using Users_API.Models;
using Users_API.Repository;
using Users_API.Services.Interface;

namespace Users_API.Controllers
{
    [Route("api/UserService/{userId}/wishlist")]
    [ApiController]
    public class WishListController : ControllerBase
    {
        private readonly IRepository<WishList> _wishListRepository;
        private readonly IMapperService _mapperService;
        private readonly HttpClient _httpClient;

        public WishListController(IRepository<WishList> wishListRepository, IMapperService mapperService, IHttpClientFactory httpClientFactory)
        {
            _wishListRepository = wishListRepository;
            _mapperService = mapperService;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7131/api/BookService/Book/");
        }

        // GET: api/users/{userId}/wishlist
        [HttpGet]
        public async Task<ActionResult> GetWishListByUserId(int userId)
        {
            try
            {
                var wishListItems = await _wishListRepository.GetByCondition(w => w.UserID == userId);

                List<int> bookIds = wishListItems.Select(w => w.BookID ?? 0).ToList();

                if (!bookIds.Any())
                {
                    return NotFound("No items found in wishlist");
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
            catch (Exception ex)
            {
                // Log the exception (logging not shown here)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // POST: api/users/{userId}/wishlist
        [HttpPost]
        public async Task<ActionResult<WishList>> AddToWishList(int userId, int bookId)
        {
            try
            {
                var wishList = await _wishListRepository.GetFirstByCondition(w => w.UserID == userId && w.BookID == bookId, b => b.Book);
                if (wishList == null)
                {
                    wishList = new WishList { UserID = userId, BookID = bookId };
                    await _wishListRepository.Add(wishList);
                }
                else
                {
                    return Conflict("Book already in wishlist");
                }
                return Ok(wishList);
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown here)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        // DELETE: api/users/{userId}/wishlist/{bookId}
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> RemoveFromWishList(int userId, int bookId)
        {
            try
            {
                var wishList = await _wishListRepository.GetFirstByCondition(w => w.UserID == userId && w.BookID == bookId, b => b.Book);
                if (wishList != null)
                {
                    await _wishListRepository.Delete(wishList);
                }
                else
                {
                    return Conflict("Book not wishlisted");
                }
                return Ok(wishList);
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown here)
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
