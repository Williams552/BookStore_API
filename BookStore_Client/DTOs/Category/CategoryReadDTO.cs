using System;

namespace BookStore_Client.Domain.DTO
{
    public class CategoryReadDTO
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<BookDTO> Books { get; set; } = new List<BookDTO>();

    }
}