using System;
using System.Collections.Generic;

namespace BookStore_API.Models;

public partial class Book
{
    public int BookID { get; set; }

    public int? AuthorID { get; set; }

    public int? CategoryID { get; set; }

    public int? SupplierID { get; set; }

    public string? Title { get; set; }

    public decimal? Price { get; set; }

    public int? Stock { get; set; }

    public string? Description { get; set; }

    public DateOnly? PublicDate { get; set; }

    public string? ImageURL { get; set; }

    public int? UpdateBy { get; set; }

    public DateOnly? UpdateAt { get; set; }

    public bool? IsDelete { get; set; }

    public virtual Author? Author { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Category { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
}
