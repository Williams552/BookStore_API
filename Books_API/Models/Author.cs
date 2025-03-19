using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Books_API.Models;

public partial class Author
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AuthorID { get; set; }

    public string? FullName { get; set; }

    public string? Biography { get; set; }

    public string? ImageURL { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
