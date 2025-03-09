using System;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Domain.DTO
{
    public class UserDTO
    {
        public int UserID { get; set; }
        [Required] 
        public string Username { get; set; } = string.Empty;
        [Required] 
        public string Password { get; set; } = string.Empty;
        [Required] 
        public string Fullname { get; set; } = string.Empty;
        [Required] 
        public string Role { get; set; } = string.Empty;
        [Required] 
        public string Email { get; set; } = string.Empty;
        [Required] 
        public string Phone { get; set; } = string.Empty;
        [Required] 
        public string Address { get; set; } = string.Empty;
        public System.DateTime CreateAt { get; set; }
    }
}