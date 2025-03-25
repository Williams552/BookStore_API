using System;

namespace BookStore_Client.Domain.DTO
{
    public class CartDTO
    {
        public int CartID { get; set; }

        public int? BookID { get; set; }

        public int? UserID { get; set; }

        public int? Quantity { get; set; }

        public decimal? TotalPrice { get; set; }

        public virtual BookDTO? Book { get; set; }

        public virtual UserDTO? User { get; set; }
    }
}