using System;

namespace Library_API.Domain.DTO
{
    public class OrderUpdateDTO
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public string RecipientName { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryOption { get; set; }
        public string DeliveryAddress { get; set; }
        public string recipient_phone { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}