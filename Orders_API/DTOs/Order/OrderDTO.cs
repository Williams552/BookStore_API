using Orders_API.Models;
using System;

namespace Orders_API.Domain.DTO
{
    public class OrderDTO
    {
        public int OrderID { get; set; }

        public int? UserID { get; set; }

        public DateOnly? OrderDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public int? PaymentMethod { get; set; }

        public string? Status { get; set; }
        public string? Address { get; set; }

        public ICollection<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
    }
}