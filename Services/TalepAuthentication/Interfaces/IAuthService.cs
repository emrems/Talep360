using TalepAuthentication.DTOs;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<TokenDto>> LoginAsync(LoginDto loginDto);
        Task<BaseResponse<bool>> RegisterAsync(RegisterDto registerDto);
        Task<BaseResponse<bool>> LogoutAsync();
    }
}
