﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookStore_API.Models
{
    public class Supplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime CreateAt { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }

        public Boolean IsDeleted { get; set; }

        public List<Book> Books { get; set; }
    }
}
