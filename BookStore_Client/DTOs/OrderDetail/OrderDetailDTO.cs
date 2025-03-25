using System;

namespace BookStore_Client.Domain.DTO
{
    public class OrderDetailDTO
    {
        public int? BookID { get; set; }
        public int? Quantity { get; set; }
    }
}