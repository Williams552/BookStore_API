using Books_API.Models;
using System;

namespace Books_API.Domain.DTO
{
    public class BookDTO
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

        public virtual Author? Author { get; set; }

        public virtual Category? Category { get; set; }

        public virtual Supplier? Supplier { get; set; }
    }
}