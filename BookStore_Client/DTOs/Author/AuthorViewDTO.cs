using System;

namespace BookStore_Client.Domain.DTO
{
    public class AuthorViewDTO
    {
        public int AuthorID { get; set; }
        public string? FullName { get; set; }
        public string? Biography { get; set; }
        public string? ImageURL { get; set; }
        public virtual ICollection<BookDTO> Books { get; set; } = new List<BookDTO>();

    }
}