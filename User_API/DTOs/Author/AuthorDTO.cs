using System;

namespace Users_API.Domain.DTO
{
    public class AuthorDTO
    {
        public int AuthorID { get; set; }
        public string? FullName { get; set; }
        public string? Biography { get; set; }
        public string? ImageURL { get; set; }
    }
}