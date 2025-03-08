using System;

namespace Library_API.Domain.DTO
{
    public class SupplierUpdateDTO
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public System.DateTime CreateAt { get; set; }
    }
}