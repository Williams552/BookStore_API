﻿using BookStore_API.Domain.DTO;
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

        // GET: api/cart/{id}
        [HttpGet("getCartByUserId/{id}")]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCartById(int id)
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
                if (cart == null)
                {
                    return NoContent();
                }
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

        // DELETE: api/cart/deleteCartByUser/{userId}
        [HttpDelete("deleteCartByUser/{userId}")]
        public async Task<IActionResult> DeleteCartByUser(int userId)
        {
            try
            {
                var result = await _cartService.DeleteCartByUser(userId);

                if (result)
                {
                    return Ok($"All cart items for user {userId} have been deleted.");
                }

                return NotFound($"No cart items found for user {userId}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting cart items: {ex.Message}");
            }
        }
    }
}
