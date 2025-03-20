using System;

namespace Orders_API.Domain.DTO
{
    public class OrderDetailViewDTO
    {
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public BookDTO Book { get; set; }

    }
}