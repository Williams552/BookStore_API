using BookStore_Client.Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orders_API.Domain.DTO;
using Orders_API.Models;
using Orders_API.Repository;
using Orders_API.Services.Interface;
using System.Net.Http;

namespace Orders_API.Controllers
{
    [Route("api/OrderService/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IMapperService _mapperService;
        private readonly IOrderService _orderService;
        private readonly HttpClient _httpClient;

        public OrderController(IRepository<Order> orderRepository, IMapperService mapperService, IOrderService orderService, IHttpClientFactory httpClient)
        {
            _orderRepository = orderRepository;
            _mapperService = mapperService;
            _orderService = orderService;
            _httpClient = httpClient.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7202/api/");
        }

        // GET: api/order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderViewDTO>>> GetAllOrders()
        {
            var orders = await Task.Run(() => _orderRepository.GetAll(o => o.OrderDetails));
            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found.");
            }

            var orderDTOs = new List<OrderViewDTO>();
            foreach (var order in orders)
            {
                var orderDTO = _mapperService.MapToDto<Order, OrderViewDTO>(order);
                var orderDetails = orderDTO.OrderDetails.ToList();

                foreach (var orderDetail in orderDetails)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(orderDetail.BookID.ToString());
                        if (response.IsSuccessStatusCode)
                        {
                            var book = await response.Content.ReadFromJsonAsync<BookDTO>();
                            orderDetail.Book = book;
                        }
                        else
                        {
                            orderDetail.Book = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nếu cần
                        orderDetail.Book = null;
                    }
                }

                orderDTO.OrderDetails = orderDetails;
                orderDTOs.Add(orderDTO);
            }

            return Ok(orderDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderViewDTO>> GetOrderById(int id)
        {
            var order = await Task.Run(() => _orderRepository.GetById(id, o => o.OrderDetails));

            var orderDTO = _mapperService.MapToDto<Order, OrderViewDTO>(order);

            var orderDetails = orderDTO.OrderDetails.ToList();

            foreach (var orderDetail in orderDetails)
            {
                var response = await _httpClient.GetAsync(orderDetail.BookID.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var book = await response.Content.ReadFromJsonAsync<BookDTO>();
                    orderDetail.Book = book;
                }
            }

            orderDTO.OrderDetails = orderDetails;

            return Ok(orderDTO);
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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderViewDTO>>> GetOrdersByUser(int userId)
        {
            var orders = await Task.Run(() => _orderRepository.GetByCondition(o => o.UserID == userId, o => o.OrderDetails));

            if (orders == null || !orders.Any())
            {
                return NotFound($"No orders found for user with ID {userId}.");
            }

            var orderDTOs = orders.Select(order =>
            {
                var orderDTO = _mapperService.MapToDto<Order, OrderViewDTO>(order);
                var orderDetails = orderDTO.OrderDetails.ToList();

                foreach (var orderDetail in orderDetails)
                {
                    var response = _httpClient.GetAsync(orderDetail.BookID.ToString()).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var book = response.Content.ReadFromJsonAsync<BookDTO>().Result;
                        orderDetail.Book = book;
                    }
                }

                orderDTO.OrderDetails = orderDetails;
                return orderDTO;
            }).ToList();

            return Ok(orderDTOs);
        }
        // Trong Orders_API.Controllers.OrderController
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            try
            {
                var existingOrder = await _orderRepository.GetById(id, o => o.OrderDetails);
                if (existingOrder == null)
                {
                    return NotFound($"Order with ID {id} not found.");
                }

                if (existingOrder.Status != "Pending")
                {
                    return BadRequest("Only orders with 'Pending' status can be confirmed.");
                }

                // Gọi API cập nhật stock cho từng sách
                foreach (var detail in existingOrder.OrderDetails)
                {
                    var response = await _httpClient.PutAsJsonAsync(
                        $"Book/updateStock/{detail.BookID}",
                        detail.Quantity);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Rollback xác nhận đơn hàng nếu có lỗi
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return BadRequest($"Failed to update stock for book {detail.BookID}: {errorContent}");
                    }
                }

                existingOrder.Status = "Accept";
                await _orderRepository.Update(existingOrder);

                return Ok(existingOrder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("earnings")]
        public async Task<ActionResult<EarningsDTO>> GetEarnings([FromQuery] int year, [FromQuery] int month)
        {
            // Kiểm tra giá trị hợp lệ
            if (month < 1 || month > 12)
            {
                return BadRequest("Month must be between 1 and 12.");
            }
            if (year < 1900 || year > DateTime.Now.Year)
            {
                return BadRequest($"Year must be between 1900 and {DateTime.Now.Year}.");
            }

            try
            {
                var earnings = await _orderService.GetEarnings(year, month);
                return Ok(earnings);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving earnings: {ex.Message}");
            }
        }
    }
}
