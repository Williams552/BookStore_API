using System;

namespace Library_API.Domain.DTO
{
    public class CategoryUpdateDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}