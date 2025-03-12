using BookStore_API.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Domain.DTO
{
    public class AuthorViewDTO
    {
        public int AuthorID { get; set; }

        public string Fullname { get; set; } = null!;

        public string Biography { get; set; } = null!;

        public string? ImageURL { get; set; }

        public string CreateBy { get; set; } = null!;

        public DateTime CreateAt { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateAt { get; set; }

        public string? DeleteBy { get; set; }

        public DateTime? DeleteAt { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<BookDTO> Books { get; set; } = new List<BookDTO>();
    }
}