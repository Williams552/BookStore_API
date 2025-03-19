using Books_API.Models;
using System;

namespace Books_API.Domain.DTO
{
    public class CategoryReadDTO
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<BookDTO> Books { get; set; } = new List<BookDTO>();

    }
}