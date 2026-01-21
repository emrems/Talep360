using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalepAuthentication.DbContext;
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
        private readonly AppDbContext _context;

        public RoleService(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<BaseResponse<bool>> AssignRoleAsync(AssignRoleDto assignRoleDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
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

                var userHasRole = await _userManager.IsInRoleAsync(user, assignRoleDto.RoleName);
                if (userHasRole)
                {
                    return BaseResponse<bool>.Fail("Kullanıcı zaten bu role sahip.", "USER_ALREADY_HAS_ROLE");
                }

                var result = await _userManager.AddToRoleAsync(user, assignRoleDto.RoleName);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BaseResponse<bool>.Fail($"Rol atama işlemi başarısız: {errors}", "ROLE_ASSIGNMENT_FAILED");
                }

                await transaction.CommitAsync();
                return BaseResponse<bool>.Success(true, "Rol başarıyla atandı.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BaseResponse<bool>.Fail($"Bir hata oluştu: {ex.Message}", "INTERNAL_ERROR");
            }
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

        public async Task<BaseResponse<List<UserDto>>> GetUsersInRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName
            }).ToList();

            return BaseResponse<List<UserDto>>.Success(userDtos, $"'{roleName}' rolündeki kullanıcılar getirildi.");
        }
    }
}
