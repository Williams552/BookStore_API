namespace BookStore_API.DTOs.Auth
{
    public class GoogleLoginRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
