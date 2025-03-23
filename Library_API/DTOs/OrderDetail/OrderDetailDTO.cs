using System;

namespace BookStore_API.Domain.DTO
{
    public class OrderDetailDTO
    {
        public int? BookID { get; set; }
        public int? Quantity { get; set; }
    }
}