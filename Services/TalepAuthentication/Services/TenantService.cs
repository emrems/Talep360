using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TalepAuthentication.DbContext;
using TalepAuthentication.DTOs;
using TalepAuthentication.Entities;
using TalepAuthentication.Interfaces;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Services
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public TenantService(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<BaseResponse<int>> CreateTenantAsync(CreateTenantDto createTenantDto)
        {
            // 1. Transaction başlat (hem tenant hem admin user hatasız oluşmalı)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Tenant oluştur
                var tenant = new Tenant
                {
                    Name = createTenantDto.Name,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _context.Tenants.AddAsync(tenant);
                await _context.SaveChangesAsync(); // Id oluşsun diye kaydediyoruz

                // 3. Admin kullanıcısını oluştur
                var adminUser = new User
                {
                    UserName = createTenantDto.AdminEmail,
                    Email = createTenantDto.AdminEmail,
                    FullName = createTenantDto.AdminFullName,
                    TenantId = tenant.Id,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(adminUser, createTenantDto.AdminPassword);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return BaseResponse<int>.Fail($"Admin kullanıcısı oluşturulurken hata oluştu: {errors}", "USER_CREATION_FAILED");
                }

                // 4. Kullanıcıya 'Admin' rolü ata
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BaseResponse<int>.Fail("Admin rolü atanırken hata oluştu.", "ROLE_ASSIGNMENT_FAILED");
                }

                await transaction.CommitAsync();

                return BaseResponse<int>.Success(tenant.Id, "Şirket ve yönetici hesabı başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BaseResponse<int>.Fail($"Bir hata oluştu: {ex.Message}", "INTERNAL_ERROR");
            }
        }

        public async Task<BaseResponse<bool>> UpdateTenantAsync(UpdateTenantDto updateTenantDto)
        {
            var tenant = await _context.Tenants.FindAsync(updateTenantDto.Id);
            if (tenant == null)
            {
                return BaseResponse<bool>.Fail("Şirket bulunamadı.", "TENANT_NOT_FOUND");
            }

            tenant.Name = updateTenantDto.Name;
            tenant.IsActive = updateTenantDto.IsActive;

            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Şirket bilgileri güncellendi.");
        }

        public async Task<BaseResponse<bool>> DeleteTenantAsync(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return BaseResponse<bool>.Fail("Şirket bulunamadı.", "TENANT_NOT_FOUND");
            }

            // Soft delete: IsActive = false
            tenant.IsActive = false;

            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Şirket pasife alındı (silindi).");
        }

        public async Task<BaseResponse<List<TenantDto>>> GetAllTenantsAsync()
        {
            var tenants = await _context.Tenants
                .Select(t => new TenantDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsActive = t.IsActive,
                    CreatedAtUtc = t.CreatedAtUtc
                })
                .ToListAsync();

            return BaseResponse<List<TenantDto>>.Success(tenants, "Şirketler listelendi.");
        }

        public async Task<BaseResponse<TenantDto>> GetTenantByIdAsync(int id)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
            {
                return BaseResponse<TenantDto>.Fail("Şirket bulunamadı.", "TENANT_NOT_FOUND");
            }

            var tenantDto = new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                IsActive = tenant.IsActive,
                CreatedAtUtc = tenant.CreatedAtUtc
            };

            return BaseResponse<TenantDto>.Success(tenantDto, "Şirket bilgileri getirildi.");
        }
    }
}