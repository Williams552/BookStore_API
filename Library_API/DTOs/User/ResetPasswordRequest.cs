using System.ComponentModel.DataAnnotations;

namespace BookStore_API.DTOs.User
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [MinLength(5, ErrorMessage = "New Password must be at least 5 characters long.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
