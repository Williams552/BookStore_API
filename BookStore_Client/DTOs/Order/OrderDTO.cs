using BookStore_Client.Domain.DTO;
using System;

namespace BookStore_API.Domain.DTO
{
    public class OrderDTO
    {
        public int OrderID { get; set; }
        public string? Status { get; set; }
        public ICollection<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
    }
}