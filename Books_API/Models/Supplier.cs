using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Books_API.Models;

public partial class Supplier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SupplierID { get; set; }

    public string? SupplierName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
