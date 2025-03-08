using System;

namespace Library_API.Domain.DTO
{
    public class OrderDetailReadDTO
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}