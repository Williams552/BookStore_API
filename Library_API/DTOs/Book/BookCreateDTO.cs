using System;

namespace Library_API.Domain.DTO
{
    public class BookCreateDTO
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public int AuthorID { get; set; }
        public int CategoryID { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }
        public int SupplierID { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}