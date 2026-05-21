namespace TalepAuthentication.DTOs
{
    public class CreateUserDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Title { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
