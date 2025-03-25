using System;

namespace BookStore_API.Domain.DTO
{
    public class OrderReadDTO
    {
        public int OrderID { get; set; }
        public string? Status { get; set; }
    }
}