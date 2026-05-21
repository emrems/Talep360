using TalepAuthentication.DTOs;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Interfaces
{
    public interface IUserService
    {
        Task<BaseResponse<bool>> CreateUserAsync(CreateUserDto createUserDto, int adminTenantId);
        Task<BaseResponse<bool>> UpdateUserAsync(UpdateUserDto updateUserDto, int adminTenantId);
        Task<BaseResponse<bool>> DeleteUserAsync(int userId, int adminTenantId);
        Task<BaseResponse<List<UserDto>>> GetTenantUsersAsync(int tenantId);
    }
}
