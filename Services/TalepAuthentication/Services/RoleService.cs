using Microsoft.AspNetCore.Identity;
using TalepAuthentication.DTOs;
using TalepAuthentication.Entities;
using TalepAuthentication.Interfaces;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Services
{
    public class RoleService : IRoleService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public RoleService(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<BaseResponse<bool>> AssignRoleAsync(AssignRoleDto assignRoleDto)
        {
            var user = await _userManager.FindByEmailAsync(assignRoleDto.UserEmail);
            if (user == null)
            {
                return BaseResponse<bool>.Fail("Kullanıcı bulunamadı.", "USER_NOT_FOUND");
            }

            var roleExists = await _roleManager.RoleExistsAsync(assignRoleDto.RoleName);
            if (!roleExists)
            {
                return BaseResponse<bool>.Fail("Belirtilen rol sistemde mevcut değil.", "ROLE_NOT_FOUND");
            }

            var result = await _userManager.AddToRoleAsync(user, assignRoleDto.RoleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BaseResponse<bool>.Fail($"Rol atama işlemi başarısız: {errors}", "ROLE_ASSIGNMENT_FAILED");
            }

            return BaseResponse<bool>.Success(true, "Rol başarıyla atandı.");
        }

        public async Task<BaseResponse<IList<string>>> GetUserRolesAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BaseResponse<IList<string>>.Fail("Kullanıcı bulunamadı.", "USER_NOT_FOUND");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return BaseResponse<IList<string>>.Success(roles, "Kullanıcı rolleri başarıyla getirildi.");
        }
    }
}
