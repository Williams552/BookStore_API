using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
		public DateTime CreateAt { get; set; }
		public string? UpdateBy { get; set; }
		public DateTime? UpdateAt { get; set; }
		public string? DeleteBy { get; set; }
		public DateTime? DeleteAt { get; set; }

		public Boolean IsDeleted { get; set; }

		public List<Order> Orders { get; set; }
        public List<Cart> Carts { get; set; }
    }
}
