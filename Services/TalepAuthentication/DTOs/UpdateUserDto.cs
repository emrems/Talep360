namespace TalepAuthentication.DTOs
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? Title { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
    }
}