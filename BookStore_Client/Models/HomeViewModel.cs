using BookStore_Client.Models;

namespace BookStore_Client.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}