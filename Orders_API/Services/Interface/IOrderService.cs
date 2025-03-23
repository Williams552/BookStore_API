using Orders_API.Domain.DTO;
using Orders_API.Models;

namespace Orders_API.Services.Interface
{
    public interface IOrderService
    {
        Task<Order> CreateOrder(OrderDTO orderDTO);
    }

}
