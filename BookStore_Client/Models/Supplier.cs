namespace BookStore_Client.Models;

public partial class Supplier
{
    public int SupplierID { get; set; }

    public string? SupplierName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
