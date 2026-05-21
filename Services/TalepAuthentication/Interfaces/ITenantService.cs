using TalepAuthentication.DTOs;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Interfaces
{
    public interface ITenantService
    {
        Task<BaseResponse<int>> CreateTenantAsync(CreateTenantDto createTenantDto);
        Task<BaseResponse<bool>> UpdateTenantAsync(UpdateTenantDto updateTenantDto);
        Task<BaseResponse<bool>> DeleteTenantAsync(int id);
        Task<BaseResponse<List<TenantDto>>> GetAllTenantsAsync();
        Task<BaseResponse<TenantDto>> GetTenantByIdAsync(int id);
    }
}