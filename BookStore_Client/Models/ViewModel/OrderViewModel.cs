namespace BookStore_Client.Models.ViewModel
{
    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public int? UserID { get; set; }
        public DateOnly? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public string UserFullName { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
    }
}
