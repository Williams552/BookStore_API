namespace BookStore_Client.Models.ViewModels
{
    public class BookViewModel
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime? PublicDate { get; set; }
        public string ImageURL { get; set; }
        public int? UpdateBy { get; set; }

        public DateTime? UpdateAt { get; set; }

        // Thông tin liên quan (thay vì ID)
        public string AuthorName { get; set; }
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }

        // ID để xử lý form
        public int AuthorID { get; set; }
        public int CategoryID { get; set; }
        public int SupplierID { get; set; }

        public Book Book { get; set; } = new();

        public List<Author> Authors { get; set; } = new List<Author>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}
