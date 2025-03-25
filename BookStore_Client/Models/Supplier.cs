using System.ComponentModel.DataAnnotations;

namespace BookStore_Client.Models;

public partial class Supplier
{
    public int SupplierID { get; set; }

    [Required(ErrorMessage = "Tên nhà cung cấp không được để trống.")]
    public string? SupplierName { get; set; }

    [Required(ErrorMessage = "Số điện thoại không được để trống.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và đủ 10 chữ số.")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Địa chỉ không được để trống.")]
    public string? Address { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}