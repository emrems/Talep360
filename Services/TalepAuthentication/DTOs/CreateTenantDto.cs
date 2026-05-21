namespace TalepAuthentication.DTOs
{
    public class CreateTenantDto
    {
        public string Name { get; set; } = null!;
        public string AdminEmail { get; set; } = null!;
        public string AdminPassword { get; set; } = null!;
        public string AdminFullName { get; set; } = null!;
    }
}