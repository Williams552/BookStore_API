﻿namespace BookStore_Client.DTOs
{
    internal class PaymentInformationDTO
    {
        public string OrderType { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
    }
}