using System;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Domain.DTO
{
    public class SupplierDTO
    {
        public int SupplierID { get; set; }
        [Required]
        public string SupplierName { get; set; } = string.Empty;
        [Required] 
        public string ContactName { get; set; } = string.Empty;
        [Required] 
        public string Phone { get; set; } = string.Empty;
        [Required] 
        public string Email { get; set; } = string.Empty;
        [Required] 
        public string Address { get; set; } = string.Empty;
        public System.DateTime CreateAt { get; set; }
    }
}