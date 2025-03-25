using BookStore_Client.Models.ViewModels;

namespace BookStore_Client.Models.ViewModel
{
    public class OrderDetailViewModel
    {
        public int BookID { get; set; }
        public int Quantity { get; set; }
        public BookViewModel Book { get; set; }
    }
}
