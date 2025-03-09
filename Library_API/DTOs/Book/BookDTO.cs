using System;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Domain.DTO
{
    public class BookDTO
    {
        public int BookID { get; set; }
        [Required] 
        public string Title { get; set; } = string.Empty;
        public int AuthorID { get; set; }
        public int CategoryID { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        [Required] 
        public string Description { get; set; } = string.Empty;
        [Required] 
        public string ImageURL { get; set; } = string.Empty; 
        public int SupplierID { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}