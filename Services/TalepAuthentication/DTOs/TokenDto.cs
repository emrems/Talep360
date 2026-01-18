namespace TalepAuthentication.DTOs
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}
