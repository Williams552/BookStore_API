using System;

namespace BookStore_Client.Domain.DTO
{
    public class UserReadDTO
    {
        public int UserID { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? ImageUrl { get; set; }
    }
}