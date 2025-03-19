using System;

namespace Books_API.Domain.DTO
{
    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }
}