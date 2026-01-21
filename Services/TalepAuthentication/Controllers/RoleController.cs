using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalepAuthentication.DTOs;
using TalepAuthentication.Interfaces;
using TalepAuthentication.Wrappers;

namespace TalepAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost("assign-role")]
        public async Task<ActionResult<BaseResponse<bool>>> AssignRole([FromBody] AssignRoleDto assignRoleDto)
        {
            var response = await _roleService.AssignRoleAsync(assignRoleDto);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        // sistemde belirli bir kullanıcının rollerini almak için kullanılır.
        [HttpGet("get-user-roles")]
        public async Task<ActionResult<BaseResponse<IList<string>>>> GetUserRoles([FromQuery] string email)
        {
            var response = await _roleService.GetUserRolesAsync(email);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        // görev atayacağım zaman kullanıcılara göre rollerini almak için kullanılır.
        [HttpGet("users-in-role")]
        public async Task<ActionResult<BaseResponse<List<UserDto>>>> GetUsersInRole([FromQuery] string roleName)
        {
            var response = await _roleService.GetUsersInRoleAsync(roleName);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
