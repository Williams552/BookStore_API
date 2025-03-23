using Orders_API.Domain.DTO;
using System;
using System.Collections.Generic;

namespace Orders_API.Models;

public partial class OrderDetail
{
    public int OrderDetailID { get; set; }

    public int? OrderID { get; set; }

    public int? BookID { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public virtual Order Order { get; set; }
}
