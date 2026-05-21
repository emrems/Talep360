using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalepAuthentication.DTOs;
using TalepAuthentication.Interfaces;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin,Manager")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !int.TryParse(tenantIdClaim, out int tenantId))
            {
                return Unauthorized(BaseResponse<bool>.Fail("Tenant bilgisi bulunamadı.", "UNAUTHORIZED"));
            }

            var result = await _userService.CreateUserAsync(createUserDto, tenantId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !int.TryParse(tenantIdClaim, out int tenantId))
            {
                return Unauthorized(BaseResponse<bool>.Fail("Tenant bilgisi bulunamadı.", "UNAUTHORIZED"));
            }

            var result = await _userService.UpdateUserAsync(updateUserDto, tenantId);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !int.TryParse(tenantIdClaim, out int tenantId))
            {
                return Unauthorized(BaseResponse<bool>.Fail("Tenant bilgisi bulunamadı.", "UNAUTHORIZED"));
            }

            var result = await _userService.DeleteUserAsync(id, tenantId);
            if (result.IsSuccess) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetTenantUsers()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !int.TryParse(tenantIdClaim, out int tenantId))
            {
                return Unauthorized(BaseResponse<List<UserDto>>.Fail("Tenant bilgisi bulunamadı.", "UNAUTHORIZED"));
            }

            var result = await _userService.GetTenantUsersAsync(tenantId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("tenant/{tenantId}/users")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetUsersByTenantId(int tenantId)
        {
            var result = await _userService.GetTenantUsersAsync(tenantId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
