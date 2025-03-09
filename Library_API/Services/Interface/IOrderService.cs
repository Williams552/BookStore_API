using BookStore_API.Domain.DTO;
using BookStore_API.Models;

namespace BookStore_API.Services.Interface
{
    public interface IOrderService
    {
        Task<Order> CreateOrder(OrderDTO orderDTO);
    }

}
