using Newtonsoft.Json;

namespace BookStore_Client.DTOs
{
    public class TokenResponseDTO
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
