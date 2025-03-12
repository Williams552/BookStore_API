using System;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Domain.DTO
{
    public class AuthorDTO
    {
        public int AuthorID { get; set; }
        [Required]
        public string Fullname { get; set; } = string.Empty;
        [Required]
        public string Biography { get; set; } = string.Empty;
        public string? ImageURL { get; set; }
        public string? ByID { get; set; }

        public DateTime? Time { get; set; }
    }
}
