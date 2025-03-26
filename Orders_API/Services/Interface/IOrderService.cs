using BookStore_Client.Domain.DTO;
using Orders_API.Domain.DTO;
using Orders_API.Models;

namespace Orders_API.Services.Interface
{
    public interface IOrderService
    {
        Task<Order> CreateOrder(OrderDTO orderDTO);

        Task<EarningsDTO> GetEarnings(int year, int month);
    }

}
