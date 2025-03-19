using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Users_API.Domain.DTO;

namespace Users_API.Models;

public partial class WishList
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int WishID { get; set; }

    public int? BookID { get; set; }

    public int? UserID { get; set; }

    public virtual BookDTO? Book { get; set; }

    public virtual User? User { get; set; }
}
