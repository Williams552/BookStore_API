using System;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Domain.DTO
{
    public class OrderDTO
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        [Required] 
        public string RecipientName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        [Required] 
        public string DeliveryOption { get; set; } = string.Empty;
        [Required] 
        public string DeliveryAddress { get; set; } = string.Empty;
        [Required] 
        public string recipient_phone { get; set; } = string.Empty;
        [Required] 
        public string PaymentMethod { get; set; } = string.Empty;
        [Required] 
        public string Status { get; set; } = string.Empty;
        public System.DateTime CreateAt { get; set; }
    }
}