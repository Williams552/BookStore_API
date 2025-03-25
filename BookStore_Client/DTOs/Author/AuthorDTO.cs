using System;

namespace BookStore_Client.Domain.DTO
{
    public class AuthorDTO
    {
        public int AuthorID { get; set; }
        public string? FullName { get; set; }
        public string? Biography { get; set; }
        public string? ImageURL { get; set; }
    }
}