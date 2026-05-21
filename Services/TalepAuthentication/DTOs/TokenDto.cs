namespace TalepAuthentication.DTOs
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int TenantId { get; set; }
        public string TenantName { get; set; } = null!;
    }
}