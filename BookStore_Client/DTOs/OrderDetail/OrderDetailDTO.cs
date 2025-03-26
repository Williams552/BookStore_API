using System;

namespace BookStore_Client.Domain.DTO
{
    public class OrderDetailDTO
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public BookDTO Book { get; set; }
    }
}