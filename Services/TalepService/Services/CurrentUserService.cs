using System.Security.Claims;
using TalepService.Interfaces.Services;

namespace TalepService.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            }
        }

        public int TenantId
        {
            get
            {
                var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId");
                return tenantIdClaim != null ? int.Parse(tenantIdClaim.Value) : 0;
            }
        }

        public bool IsSuperAdmin
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.IsInRole("SuperAdmin") ?? false;
            }
        }

        public bool IsStaff
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.IsInRole("Staff") ?? false;
            }
        }
    }
}
