using System;

namespace BookStore_Client.Domain.DTO
{
    public class SupplierReadDTO
    {
        public int SupplierID { get; set; }
        public string? SupplierName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public virtual ICollection<BookDTO> Books { get; set; } = new List<BookDTO>();

    }
}