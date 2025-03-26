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
        public decimal? TotalAmount { get; set; } // Thêm
        public int? PaymentMethod { get; set; }  // Thêm
        public string Status { get; set; }
        public List<OrderDetailDTO> OrderDetails { get; set; }
    }
    public class OrderDetailDTO
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public BookDTO Book { get; set; }
    }

    public class BookDTO
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string ImageURL { get; set; } // Gi? ??nh API có ImageURL
    }
}