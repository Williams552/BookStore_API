
namespace BookStore_API.Models;

public partial class Order
{
    public int OrderID { get; set; }

    public int? UserID { get; set; }

    public DateOnly? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public int? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User? User { get; set; }
}
