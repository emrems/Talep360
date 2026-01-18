using System.Security.Claims;
using TalepAuthentication.DTOs;
using TalepAuthentication.Entities;

namespace TalepAuthentication.Interfaces
{
    public interface IJwtService
    {
        TokenDto GenerateToken(User user, IList<string> roles);
    }
}
