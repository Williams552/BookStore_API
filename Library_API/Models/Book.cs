using System;
using System.Collections.Generic;

namespace BookStore_API.Models;

public partial class Book
{
    public int BookID { get; set; }

    public string Title { get; set; } = null!;

    public int AuthorID { get; set; }

    public int CategoryID { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Description { get; set; } = null!;

    public string ImageURL { get; set; } = null!;

    public int SupplierID { get; set; }

    public DateTime CreateAt { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? DeleteBy { get; set; }

    public DateTime? DeleteAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Supplier Supplier { get; set; } = null!;
}
