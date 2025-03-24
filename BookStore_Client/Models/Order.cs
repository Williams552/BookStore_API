
<<<<<<<< HEAD:Orders_API/Models/Order.cs
using Orders_API.Domain.DTO;
using Orders_API.Models;

namespace Orders_API.Models;
========
namespace BookStore_Client.Models;
>>>>>>>> Client:BookStore_Client/Models/Order.cs

public partial class Order
{
    public int OrderID { get; set; }

    public int? UserID { get; set; }

    public DateOnly? OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public int? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
