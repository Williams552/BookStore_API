using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Library_API.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public string RecipientName { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryOption { get; set; }
        public string DeliveryAddress { get; set; }
        public string recipient_phone { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public DateTime? DeleteAt { get; set; }

        public User User { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
