using System;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Domain.DTO
{
    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        [Required] 
        public string CategoryName { get; set; } = string.Empty;
        [Required] 
        public string Description { get; set; } = string.Empty;
        public System.DateTime CreateAt { get; set; }
    }
}