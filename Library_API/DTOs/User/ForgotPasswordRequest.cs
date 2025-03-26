using System.ComponentModel.DataAnnotations;

namespace BookStore_API.DTOs.User
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "The Email field is required.")]
        public string Email { get; set; }
    }
}
