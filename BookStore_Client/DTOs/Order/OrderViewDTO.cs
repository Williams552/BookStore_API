using System;

namespace BookStore_Client.Domain.DTO

{
    public class OrderViewDTO
    {
        public int OrderID { get; set; }

        public int? UserID { get; set; }

        public DateOnly? OrderDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public int? PaymentMethod { get; set; }

        public string? Status { get; set; }
        public ICollection<OrderDetailViewDTO> OrderDetails { get; set; } = new List<OrderDetailViewDTO>();
    }
}