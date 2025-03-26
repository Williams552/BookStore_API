using System;

namespace BookStore_Client.Domain.DTO
{
    public class OrderDetailViewDTO
    {
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public BookDTO Book { get; set; }

    }
}