namespace BookStore_API.Models;

public partial class WishList
{
    public int WishID { get; set; }

    public int? BookID { get; set; }

    public int? UserID { get; set; }

    public virtual Book? Book { get; set; }

    public virtual User? User { get; set; }
}
