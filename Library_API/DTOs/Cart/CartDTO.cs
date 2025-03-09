using System;

namespace BookStore_API.Domain.DTO
{
    public class CartDTO
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}