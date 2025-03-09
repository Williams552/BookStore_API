using System;
using System.Collections.Generic;

namespace BookStore_API.Models;

public partial class Category
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public string? UpdateBy { get; set; }

    public DateTime? UpdateAt { get; set; }

    public string? DeleteBy { get; set; }

    public DateTime? DeleteAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
