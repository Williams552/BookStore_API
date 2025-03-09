using System;
using System.Collections.Generic;

namespace BookStore_API.Models;

public partial class OrderDetail
{
    public int OrderDetailID { get; set; }

    public int OrderID { get; set; }

    public int BookID { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime CreateAt { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? DeleteBy { get; set; }

    public DateTime? DeleteAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
