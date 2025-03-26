
    namespace BookStore_Client.Models.ViewModel
    {
        public class OrderViewModel
        {
            public int OrderID { get; set; }
            public int? UserID { get; set; }
            public DateOnly? OrderDate { get; set; }
            public decimal? TotalAmount { get; set; }
            public int? PaymentMethod { get; set; }
            public string? Status { get; set; }
            public string? UserFullName { get; set; }
            public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
        }

        public class OrderDetailViewModel
        {
            public int OrderDetailID { get; set; }
            public int OrderID { get; set; }
            public int BookID { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public BookViewModel Book { get; set; }
        }

        public class BookViewModel
        {
            public int BookID { get; set; }
            public string Title { get; set; }
            public string ImageURL { get; set; } // Thêm để khớp với view
        }
    }

