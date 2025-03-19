using System;

namespace BookStore_API.Domain.DTO
{
    public class Users_API
    {
        public int BookID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageURL { get; set; }
    }
}