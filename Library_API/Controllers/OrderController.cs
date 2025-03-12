using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IMapperService _mapperService;
        private readonly IOrderService _orderService;

        public OrderController(IRepository<Order> orderRepository, IMapperService mapperService, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _mapperService = mapperService;
            _orderService = orderService;
        }

        // GET: api/order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            var orders = await Task.Run(() => _orderRepository.GetAll());
            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found.");
            }
            return Ok(orders);
        }

        // GET: api/order/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await Task.Run(() => _orderRepository.GetById(id));
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }
            return Ok(order);
        }

        // POST: api/order
        [HttpPost]
        public async Task<ActionResult<Order>> AddOrder(OrderDTO orderDTO)
        {
            try
            {
                // Call the service to create the order
                var order = await _orderService.CreateOrder(orderDTO);

                // Return the created order with a status of 201 (Created)
                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderID }, order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/order/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, OrderDTO orderDTO)
        {
            if (id != orderDTO.OrderID)
            {
                return BadRequest("Order ID mismatch.");
            }

            var existingOrder = await Task.Run(() => _orderRepository.GetById(id));
            if (existingOrder == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            var order = _mapperService.MapToDto<OrderDTO, Order>(orderDTO);

            await Task.Run(() => _orderRepository.Update(order));
            return NoContent();
        }

        // DELETE: api/order/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await Task.Run(() => _orderRepository.GetById(id));
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            await Task.Run(() => _orderRepository.Delete(order));
            return NoContent();
        }
    }
}
