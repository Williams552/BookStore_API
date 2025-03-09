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

        public CartController(IRepository<Cart> cartRepository, IMapperService mapperService)
        {
            _cartRepository = cartRepository;
            _mapperService = mapperService;
        }

        // GET: api/cart
        [HttpGet]
        public ActionResult<IEnumerable<Cart>> GetAllCarts()
        {
            var carts = _cartRepository.GetAll();
            if (carts == null || !carts.Any())
            {
                return NotFound("No carts found.");
            }
            return Ok(carts);
        }

        // GET: api/cart/{id}
        [HttpGet("{id}")]
        public ActionResult<Cart> GetCartById(int id)
        {
            var cart = _cartRepository.GetById(id);
            if (cart == null)
            {
                return NotFound($"Cart with ID {id} not found.");
            }
            return Ok(cart);
        }

        // POST: api/cart
        [HttpPost]
        public async Task<ActionResult<Cart>> AddCart(CartDTO cartDTO)
        {
            var cart = _mapperService.Map<CartDTO, Cart>(cartDTO);
            if (cart == null)
            {
                return BadRequest("Cart cannot be null.");
            }

            await Task.Run(()=> _cartRepository.Add(cart));
            return CreatedAtAction(nameof(GetCartById), new { id = cart.CartID }, cart);
        }

        // PUT: api/cart/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateCart(int id, CartDTO cartDTO)
        {
            if (id != cartDTO.CartID)
            {
                return BadRequest("Cart ID mismatch.");
            }

            var existingCart = _cartRepository.GetById(id);
            if (existingCart == null)
            {
                return NotFound($"Cart with ID {id} not found.");
            }

            var cart = _mapperService.Map<CartDTO, Cart>(cartDTO);

            _cartRepository.Update(cart);
            return NoContent();
        }

        // DELETE: api/cart/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteCart(int id)
        {
            var cart = _cartRepository.GetById(id);
            if (cart == null)
            {
                return NotFound($"Cart with ID {id} not found.");
            }

            _cartRepository.Delete(cart);
            return NoContent();
        }
    }
}
