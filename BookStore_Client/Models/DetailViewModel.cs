using System.Collections.Generic;

namespace BookStore_Client.Models
{
    public class DetailViewModel
    {
        public Book Book { get; set; }
        public List<Book> RelatedBooks { get; set; }
    }
}
