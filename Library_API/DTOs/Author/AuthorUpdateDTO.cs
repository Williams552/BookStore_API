using System;

namespace Library_API.Domain.DTO
{
    public class AuthorUpdateDTO
    {
        public int AuthorID { get; set; }
        public string Fullname { get; set; }
        public string Biography { get; set; }
        public string? ImageURL { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}