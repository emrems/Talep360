using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalepAuthentication.DbContext;
using TalepAuthentication.DTOs;
using TalepAuthentication.Entities;
using TalepAuthentication.Interfaces;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly AppDbContext _context;

        public UserService(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<BaseResponse<bool>> CreateUserAsync(CreateUserDto createUserDto, int adminTenantId)
        {
            // Transaction başlat
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Rol kontrolü
                foreach (var role in createUserDto.Roles)
                {
                    if (string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                    {
                        return BaseResponse<bool>.Fail("SuperAdmin yetkisi verilemez.", "FORBIDDEN_ROLE");
                    }

                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        return BaseResponse<bool>.Fail($"Belirtilen rol bulunamadı: {role}", "ROLE_NOT_FOUND");
                    }
                }

                // 2. Email kontrolü
                var existingUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (existingUser != null)
                {
                    return BaseResponse<bool>.Fail("Bu email adresi zaten kullanımda.", "EMAIL_ALREADY_EXISTS");
                }

                // 3. Kullanıcı oluşturma
                var user = new User
                {
                    UserName = createUserDto.Email,
                    Email = createUserDto.Email,
                    FullName = createUserDto.FullName,
                    Title = createUserDto.Title,
                    TenantId = adminTenantId, // Admin'in tenant'ına zorla
                    CreatedAtUtc = DateTime.UtcNow,
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user, createUserDto.Password);
                if (!createResult.Succeeded)
                {
                     var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                     return BaseResponse<bool>.Fail($"Kullanıcı oluşturulamadı: {errors}", "USER_CREATION_FAILED");
                }

                // 4. Rol atama
                if (createUserDto.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, createUserDto.Roles);
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception("Kullanıcı oluşturuldu ancak roller atanamadı.");
                    }
                }

                // Her şey başarılı, commit et
                await transaction.CommitAsync();
                return BaseResponse<bool>.Success(true, "Kullanıcı başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BaseResponse<bool>.Fail($"İşlem sırasında bir hata oluştu: {ex.Message}", "INTERNAL_ERROR");
            }
        }

        public async Task<BaseResponse<bool>> UpdateUserAsync(UpdateUserDto updateUserDto, int adminTenantId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByIdAsync(updateUserDto.Id.ToString());
                if (user == null || user.TenantId != adminTenantId)
                {
                    return BaseResponse<bool>.Fail("Kullanıcı bulunamadı.", "USER_NOT_FOUND");
                }

                // Rol kontrolü
                foreach (var role in updateUserDto.Roles)
                {
                    if (string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                    {
                        return BaseResponse<bool>.Fail("SuperAdmin yetkisi verilemez.", "FORBIDDEN_ROLE");
                    }
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        return BaseResponse<bool>.Fail($"Belirtilen rol bulunamadı: {role}", "ROLE_NOT_FOUND");
                    }
                }

                // Bilgileri güncelle
                user.FullName = updateUserDto.FullName;
                user.Title = updateUserDto.Title;
                user.IsActive = updateUserDto.IsActive;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return BaseResponse<bool>.Fail($"Güncelleme başarısız: {errors}", "UPDATE_FAILED");
                }

                // Rol güncelleme
                var currentRoles = await _userManager.GetRolesAsync(user);
                
                // Silinecek roller (Mevcutta var ama yeni listede yok)
                var rolesToRemove = currentRoles.Except(updateUserDto.Roles).ToList();
                if (rolesToRemove.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    if (!removeResult.Succeeded) throw new Exception("Eski roller silinemedi.");
                }

                // Eklenecek roller (Yeni listede var ama mevcutta yok)
                var rolesToAdd = updateUserDto.Roles.Except(currentRoles).ToList();
                if (rolesToAdd.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!addResult.Succeeded) throw new Exception("Yeni roller atanamadı.");
                }

                await transaction.CommitAsync();
                return BaseResponse<bool>.Success(true, "Kullanıcı başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BaseResponse<bool>.Fail($"Güncelleme sırasında hata: {ex.Message}", "INTERNAL_ERROR");
            }
        }

        public async Task<BaseResponse<bool>> DeleteUserAsync(int userId, int adminTenantId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.TenantId != adminTenantId)
            {
                return BaseResponse<bool>.Fail("Kullanıcı bulunamadı.", "USER_NOT_FOUND");
            }

            // Soft delete
            user.IsActive = false;
            user.IsDeleted = true;
            user.DeletedAtUtc = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return BaseResponse<bool>.Success(true, "Kullanıcı başarıyla silindi.");
            }
            return BaseResponse<bool>.Fail("Silme işlemi başarısız.", "DELETE_FAILED");
        }

        public async Task<BaseResponse<List<UserDto>>> GetTenantUsersAsync(int tenantId)
        {
            var users = await _context.Users
                .Where(u => u.TenantId == tenantId && !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAtUtc)
                .ToListAsync();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Title = user.Title,
                    IsActive = user.IsActive,
                    Roles = roles.ToList() // Tüm rolleri döndür
                });
            }

            return BaseResponse<List<UserDto>>.Success(userDtos, "Kullanıcılar listelendi.");
        }
    }
}
