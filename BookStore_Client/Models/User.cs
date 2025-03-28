﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookStore_Client.Models;

public partial class User
{
    public int UserID { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Gender { get; set; }

    public string? ImageUrl { get; set; }

    public DateOnly? CreateAt { get; set; }

    public DateOnly? UpdateAt { get; set; }
    
    public bool? IsDelete { get; set; }

    public int? Role { get; set; }
    public int? OTP { get; set; }
    public DateTime? TimeOtp { get; set; }

    public bool? IsActive { get; set; } = false;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<WishList> WishLists { get; set; } = new List<WishList>();
}
public class password
{
    [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
    public string? Username { get; set; }
    public string? Email { get; set; }

    [MinLength(5, ErrorMessage = "Password must be at least 5 characters long.")]
    [Required(ErrorMessage = "Password is required.")]
    public string? PasswordHash { get; set; }
    [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]

    [NotMapped]
    [Required(ErrorMessage = "New Password is required.")]
    [MinLength(5, ErrorMessage = "New Password must be at least 5 characters long.")]
    public string? NewPassword { get; set; }


    [NotMapped]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    [Required(ErrorMessage = "Confirm Password is required.")]
    public string? ConfirmPassword { get; set; }
}
