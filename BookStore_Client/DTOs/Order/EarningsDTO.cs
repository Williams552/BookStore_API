using System;

namespace BookStore_Client.Domain.DTO
{
    public class EarningsDTO
    {
        public decimal MonthlyEarningsAfterFee { get; set; }
        public decimal TotalSales { get; set; }
    }
}
