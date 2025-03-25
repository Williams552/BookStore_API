using System;

namespace BookStore_Client.Domain.DTO
{
    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }
}