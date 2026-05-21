using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalepService.DTOs.Ticket;
using TalepService.Interfaces.Services;
using TalepService.Wrappers;

using TalepService.Filters;

namespace TalepService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<IEnumerable<TicketDto>>>> GetAll()
        {
            var result = await _ticketService.GetAllTicketsAsync();
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Idempotency]
        public async Task<ActionResult<BaseResponse<TicketDto>>> Create([FromBody] CreateTicketDto createTicketDto)
        {
             var result = await _ticketService.CreateTicketAsync(createTicketDto);
             return Ok(result);
        }


        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<BaseResponse<bool>>> RejectTicket(int id, [FromBody] RejectTicketRequest request)
        {
            var result = await _ticketService.RejectTicketAsync(id, request.Reason);
            return Ok(result);
        }

        [HttpPost("{id}/assign/{userId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<BaseResponse<bool>>> AssignTicket(int id, int userId)
        {
            var result = await _ticketService.AssignTicketAsync(id, userId);
            return Ok(result);
        }

        [HttpPost("{id}/accept-assignment")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> AcceptAssignment(int id)
        {
            var result = await _ticketService.AcceptAssignmentAsync(id);
            return Ok(result);
        }

        /*
        [HttpPost("{id}/reject-assignment")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> RejectAssignment(int id)
        {
            var result = await _ticketService.RejectAssignmentAsync(id);
            return Ok(result);
        }
        */

        [HttpPost("{id}/resolve")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> ResolveTicket(int id)
        {
            var result = await _ticketService.ResolveTicketAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/close")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<BaseResponse<bool>>> CloseTicket(int id)
        {
            var result = await _ticketService.CloseTicketAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/request-info")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> RequestInfo(int id, [FromBody] RequestInfoRequest request)
        {
            var result = await _ticketService.RequestCustomerInfoAsync(id, request.Details);
            return Ok(result);
        }

        [HttpPost("{id}/reply")]
        [Authorize]
        public async Task<ActionResult<BaseResponse<bool>>> ReplyToTicket(int id, [FromBody] ReplyTicketRequest request)
        {
            var result = await _ticketService.ReplyToTicketAsync(id, request.Message);
            return Ok(result);
        }

        [HttpGet("my-tickets")]
        public async Task<ActionResult<BaseResponse<IEnumerable<TicketDto>>>> GetMyTickets()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(BaseResponse<IEnumerable<TicketDto>>.Fail("Kullanıcı kimliği doğrulanamadı.", "UNAUTHORIZED"));
            }

            var result = await _ticketService.GetTicketsByCreatedByAsync(userId);
            return Ok(result);
        }

        [HttpGet("assigned-to-me")]
        public async Task<ActionResult<BaseResponse<IEnumerable<TicketDto>>>> GetAssignedTickets()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(BaseResponse<IEnumerable<TicketDto>>.Fail("Kullanıcı kimliği doğrulanamadı.", "UNAUTHORIZED"));
            }

            var result = await _ticketService.GetTicketsByAssignedToAsync(userId);
            return Ok(result);
        }

        [HttpGet("tenant-tickets")]
        public async Task<ActionResult<BaseResponse<IEnumerable<TicketDto>>>> GetTenantTickets()
        {
            var tenantIdClaim = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !int.TryParse(tenantIdClaim, out int tenantId))
            {
                return Unauthorized(BaseResponse<IEnumerable<TicketDto>>.Fail("Tenant bilgisi doğrulanamadı.", "UNAUTHORIZED"));
            }

            var result = await _ticketService.GetTicketsByTenantIdAsync(tenantId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<TicketDto>>> GetById(int id)
        {
            var result = await _ticketService.GetTicketByIdAsync(id);
            
            return Ok(result);
        }



        [HttpPut]
        public async Task<ActionResult<BaseResponse<TicketDto>>> Update([FromBody] UpdateTicketDto updateTicketDto)
        {
            var result = await _ticketService.UpdateTicketAsync(updateTicketDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(int id)
        {
            var result = await _ticketService.DeleteTicketAsync(id);
            return Ok(result);
        }

        [HttpPost("workload-stats")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<BaseResponse<List<UserWorkloadDto>>>> GetWorkloadStats([FromBody] List<int> userIds)
        {
            var response = await _ticketService.GetWorkloadStatsAsync(userIds);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
