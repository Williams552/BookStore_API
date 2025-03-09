using System;
using System.Collections.Generic;

namespace BookStore_API.Models;

public partial class Cart
{
    public int CartID { get; set; }

    public int UserID { get; set; }

    public int BookID { get; set; }

    public int Quantity { get; set; }

    public DateTime CreateAt { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? DeleteBy { get; set; }

    public DateTime? DeleteAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
