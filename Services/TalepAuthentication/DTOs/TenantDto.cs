namespace TalepAuthentication.DTOs
{
    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}