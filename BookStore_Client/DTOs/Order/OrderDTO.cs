using System;

namespace BookStore_Client.Domain.DTO
{
    public class OrderDTO
    {
        public int OrderID { get; set; }

        public int? UserID { get; set; }

        public DateOnly? OrderDate { get; set; }

        public decimal? TotalAmount { get; set; }

        public int? PaymentMethod { get; set; }

        public string? Status { get; set; }
        public ICollection<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
        public string? Address { get; internal set; }
    }


    public class OrderViewDTO1
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; } // Th�m
        public int? PaymentMethod { get; set; }  // Th�m
        public string Status { get; set; }
        public List<OrderDetailDTO> OrderDetails { get; set; }
    }
}