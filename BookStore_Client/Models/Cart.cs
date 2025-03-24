
namespace BookStore_Client.Models;

public partial class Cart
{
    public int CartID { get; set; }

    public int? BookID { get; set; }

    public int? UserID { get; set; }

    public int? Quantity { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual Book? Book { get; set; }

    public virtual User? User { get; set; }
}
