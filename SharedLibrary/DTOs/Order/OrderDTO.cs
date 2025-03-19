using BookStore_API.Models;
using System;

namespace BookStore_API.Domain.DTO
{
    public class OrderDTO
    {
        public int OrderID { get; set; }
        public string? Status { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}