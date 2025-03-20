using System.ComponentModel.DataAnnotations;

namespace BookStore_Client.Models;

public partial class WishList
{
    [Key]
    public int WishID { get; set; }

    public int? BookID { get; set; }

    public int? UserID { get; set; }

    public virtual Book? Book { get; set; }

    public virtual User? User { get; set; }
}
