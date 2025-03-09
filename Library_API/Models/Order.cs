using System;
using System.Collections.Generic;

namespace BookStore_API.Models;

public partial class Order
{
    public int OrderID { get; set; }

    public int UserID { get; set; }

    public string RecipientName { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string DeliveryOption { get; set; } = null!;

    public string DeliveryAddress { get; set; } = null!;

    public string RecipientPhone { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? DeleteBy { get; set; }

    public DateTime? DeleteAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User User { get; set; } = null!;
}
