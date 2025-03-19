using System;

namespace Books_API.Domain.DTO
{
    public class BookReadDTO
    {
        public int BookID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageURL { get; set; }
    }
}