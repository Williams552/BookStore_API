using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IMapperService _mapperService;
        private readonly ICartService _cartService;

        public CartController(IRepository<Cart> cartRepository, IMapperService mapperService, ICartService cartService)
        {
            _cartRepository = cartRepository;
            _mapperService = mapperService;
            _cartService = cartService;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cart>>> GetAllCarts()
        {
            var carts = await Task.Run(() => _cartRepository.GetAll());
            if (carts == null || !carts.Any())
            {
                return NotFound("No carts found.");
            }
            return Ok(carts);
        }

        // GET: api/cart/{id}
        [HttpGet("getCartByUserId{id}")]
        public async Task<ActionResult<Cart>> GetCartById(int id)
        {
            var cart = await _cartService.GetCartByUserID(id);
            if (cart == null)
            {
                return NotFound($"Cart with ID {id} not found.");
            }
            return Ok(cart);
        }

        // POST: api/cart
        [HttpPost("AddToCart")]
        public async Task<ActionResult<Cart>> AddToCart(int bookId, int userId, int quantity)
        {
            try
            {
                var cart = await _cartService.AddToCart(bookId, userId, quantity);
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

        // POST: api/cart
        [HttpPost("Upsert")]
        public async Task<ActionResult<Cart>> Upsert(int bookId, int userId, int quantity)
        {
            try
            {
                var cart = await _cartService.Upsert(bookId, userId, quantity);
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

        // PUT: api/cart/{id}
        [HttpPut("updateCart{id}")]
        public async Task<IActionResult> UpdateCart(int id, CartDTO cartDTO)
        {
            if (id != cartDTO.CartID)
            {
                return BadRequest("Cart ID mismatch.");
            }

            var existingCart = await Task.Run(() => _cartRepository.GetById(id));
            if (existingCart == null)
            {
                return NotFound($"Cart with ID {id} not found.");
            }

            var cart = _mapperService.MapToDto<CartDTO, Cart>(cartDTO);

            await Task.Run(() => _cartRepository.Update(cart));
            return NoContent();
        }

        // DELETE: api/cart/{id}
        [HttpDelete]
        public async Task<IActionResult> DeleteCart(int bookId, int userId)
        {
            var cart = await _cartService.DeleteCart(bookId, userId);
            if (cart == null)
            {
                return NotFound($"Cart not exist.");
            }
            return Ok(cart);
        }
    }
}
