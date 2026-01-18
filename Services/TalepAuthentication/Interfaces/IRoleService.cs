using TalepAuthentication.DTOs;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Interfaces
{
    public interface IRoleService
    {
        Task<BaseResponse<bool>> AssignRoleAsync(AssignRoleDto assignRoleDto);
        Task<BaseResponse<IList<string>>> GetUserRolesAsync(string email);
    }
}
