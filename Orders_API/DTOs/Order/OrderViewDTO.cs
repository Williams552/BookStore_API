using System;

namespace Orders_API.Domain.DTO
{
    public class OrderViewDTO
    {
        public int OrderID { get; set; }

        public int? UserID { get; set; }

        public DateOnly? OrderDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public int? PaymentMethod { get; set; }

        public string? Status { get; set; }
        public string? Address { get; set; }

        public ICollection<OrderDetailViewDTO> OrderDetails { get; set; } = new List<OrderDetailViewDTO>();
    }
}