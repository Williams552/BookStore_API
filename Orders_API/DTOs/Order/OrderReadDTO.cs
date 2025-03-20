using System;

namespace Orders_API.Domain.DTO
{
    public class OrderReadDTO
    {
        public int OrderID { get; set; }
        public string? Status { get; set; }
    }
}