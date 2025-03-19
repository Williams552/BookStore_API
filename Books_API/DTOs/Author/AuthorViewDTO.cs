using Books_API.Models;
using System;

namespace Books_API.Domain.DTO
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