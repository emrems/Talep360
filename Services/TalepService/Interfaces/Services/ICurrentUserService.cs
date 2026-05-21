namespace TalepService.Interfaces.Services
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        int TenantId { get; }
        bool IsSuperAdmin { get; }
        bool IsStaff { get; }
    }
}
