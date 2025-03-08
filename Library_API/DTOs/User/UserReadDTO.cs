using System;

namespace Library_API.Domain.DTO
{
    public class UserReadDTO
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}