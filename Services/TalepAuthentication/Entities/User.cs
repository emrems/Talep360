using Microsoft.AspNetCore.Identity;

namespace TalepAuthentication.Entities
{
    public class User : IdentityUser<int>
    {
        // Id, Email, PasswordHash alanları IdentityUser'dan otomatik gelecek

        public int TenantId { get; set; }
       
        public string FullName { get; set; } = null!; 
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAtUtc { get; set; }
    }
}
