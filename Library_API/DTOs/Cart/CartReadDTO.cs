using System;

namespace Library_API.Domain.DTO
{
    public class CartReadDTO
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}