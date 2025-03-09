using BookStore_API.Domain.DTO;
using BookStore_API.Models;
using BookStore_API.Repository;
using BookStore_API.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IMapperService _mapperService;

        public OrderService(IRepository<Order> orderRepository, IMapperService mapperService)
        {
            _orderRepository = orderRepository;
            _mapperService = mapperService;
        }

        public async Task<Order> CreateOrder(OrderDTO orderDTO)
        {
            // Map OrderDTO to Order entity
            var order = _mapperService.Map<OrderDTO, Order>(orderDTO);
            if (order == null)
            {
                throw new ArgumentException("Order cannot be null.");
            }

            // Create OrderDetails from OrderDTO
            var orderDetails = orderDTO.OrderDetails.Select(od => new OrderDetail
            {
                BookID = od.BookID,
                Quantity = od.Quantity,
            }).ToList();

            // Assign OrderDetails to the Order
            order.OrderDetails = orderDetails;

            // Add the Order and its OrderDetails to the repository
            await Task.Run(() => _orderRepository.Add(order));
            return order;
        }
    }


}
