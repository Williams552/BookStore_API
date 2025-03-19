using System;
using System.Collections.Generic;

namespace BookStore_API.Models;

public partial class Author
{
    public int AuthorID { get; set; }

    public string? FullName { get; set; }

    public string? Biography { get; set; }

    public string? ImageURL { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
