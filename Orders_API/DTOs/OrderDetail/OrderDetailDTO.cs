using System;

namespace Orders_API.Domain.DTO
{
    public class OrderDetailDTO
    {
        public int? BookID { get; set; }
        public int? Quantity { get; set; }
    }
}