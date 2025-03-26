using Orders_API.Domain.DTO;
using Orders_API.Models;
using Orders_API.Repository;
using Orders_API.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Linq.Expressions;
using BookStore_Client.Domain.DTO;

namespace Orders_API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IMapperService _mapperService;
        private readonly HttpClient _httpClientUserAPI;
        private readonly HttpClient _httpClientBookAPI;

        public OrderService(IRepository<Order> orderRepository, IMapperService mapperService, IHttpClientFactory httpClient)
        {
            _orderRepository = orderRepository;
            _mapperService = mapperService;
            _httpClientBookAPI = httpClient.CreateClient();
            _httpClientBookAPI.BaseAddress = new Uri("https://localhost:7131/api/BookService/Book/");
            _httpClientUserAPI = httpClient.CreateClient();
            _httpClientUserAPI.BaseAddress = new Uri("https://localhost:7135/api/UserService/User/");
        }

        public async Task<Order> CreateOrder(OrderDTO orderDTO)
        {
            // Map OrderDTO to Order entity
            var order = _mapperService.MapToDto<OrderDTO, Order>(orderDTO);
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
            await _orderRepository.Add(order);
            return order;
        }

        public async Task<EarningsDTO> GetEarnings(int year, int month)
        {
            // Lấy tất cả đơn hàng đã hoàn thành
            Expression<Func<Order, bool>> completedOrders = o => o.Status == "Accept";
            var orders = await _orderRepository.GetByCondition(completedOrders);

            // Tổng giá trị tất cả các đơn hàng (dựa trên TotalAmount)
            decimal totalSales = orders
                .Where(o => o.TotalAmount.HasValue)
                .Sum(o => o.TotalAmount.Value);

            // Doanh thu cho tháng và năm được chọn
            Expression<Func<Order, bool>> monthlyFilter = o =>
                o.Status == "Accept" &&
                o.OrderDate.HasValue &&
                o.OrderDate.Value.Year == year &&
                o.OrderDate.Value.Month == month;

            var monthlyOrders = await _orderRepository.GetByCondition(monthlyFilter);

            // Tính doanh thu tháng dựa trên TotalAmount
            decimal monthlyEarnings = monthlyOrders
                .Where(o => o.TotalAmount.HasValue)
                .Sum(o => o.TotalAmount.Value);

            return new EarningsDTO
            {
                TotalSales = totalSales,
                
                MonthlyEarningsAfterFee= monthlyEarnings
            };
        }
    }


}
