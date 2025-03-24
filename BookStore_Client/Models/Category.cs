using System;
using System.Collections.Generic;

namespace BookStore_Client.Models;

public partial class Category
{
    public int CategoryID { get; set; }

    public string? CategoryName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
