using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore_API.Models
{
    public class Author
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuthorID { get; set; }
        public string Fullname { get; set; }
        public string Biography { get; set; }
        public string? ImageURL { get; set; }
		public string CreateBy { get; set; }
		public DateTime CreateAt { get; set; }
        public string? UpdateBy { get; set; }
		public DateTime? UpdateAt { get; set; }
		public string? DeleteBy { get; set; }
		public DateTime? DeleteAt { get; set; }

        public Boolean IsDeleted { get; set; }

		public List<Book> Books { get; set; }
    }
}
