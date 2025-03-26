using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishController : Controller
    {
        private readonly IRepository<WishList> _repository;
        private readonly IMapperService _mapperService;
        private readonly IWishService _service;
        public WishController(IRepository<WishList> repository, IMapperService mapperService, IWishService wishService)
        {
            _repository = repository;
            _mapperService = mapperService;
            _service = wishService;
        }

        // GET: api/cart/{id}
        [HttpGet("getByUserId/{id}")]
        public async Task<ActionResult<IEnumerable<WishList>>> GetUserByID(int id)
        {
            var cart = await _service.GetUserByID(id);
            if (cart == null)
            {
                return NotFound($"Cart with ID {id} not found.");
            }
            return Ok(cart);
        }

        [HttpPost("AddWish")]
        public async Task<ActionResult<Cart>> AddToCart(int bookId, int userId)
        {
            try
            {
                var cart = await _service.AddWish(bookId, userId);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int bookId, int userId)
        {
            var cart = await _service.Delete(bookId, userId);
            if (cart == null)
            {
                return NotFound($"Cart not exist.");
            }
            return Ok(cart);
        }
    }
}
