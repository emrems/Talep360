using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalepAuthentication.DTOs;
using TalepAuthentication.Interfaces;

namespace TalepAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "SuperAdmin")] // Sadece SuperAdmin şirket oluşturabilir
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto createTenantDto)
        {
            var result = await _tenantService.CreateTenantAsync(createTenantDto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateTenant([FromBody] UpdateTenantDto updateTenantDto)
        {
            var result = await _tenantService.UpdateTenantAsync(updateTenantDto);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var result = await _tenantService.DeleteTenantAsync(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("list")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllTenants()
        {
            var result = await _tenantService.GetAllTenantsAsync();
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetTenantById(int id)
        {
            var result = await _tenantService.GetTenantByIdAsync(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
