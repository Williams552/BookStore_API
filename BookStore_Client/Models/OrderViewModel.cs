namespace BookStore_Client.Models
{
    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public string UserFullName { get; set; } // Hiển thị FullName thay vì UserID
        public DateOnly? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? PaymentMethod { get; set; }
        public string? Status { get; set; }
    }
}
