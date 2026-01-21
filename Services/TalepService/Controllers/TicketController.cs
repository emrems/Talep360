using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalepService.DTOs.Ticket;
using TalepService.Interfaces.Services;
using TalepService.Wrappers;

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

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<TicketDto>>> GetById(int id)
        {
            var result = await _ticketService.GetTicketByIdAsync(id);
            
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<TicketDto>>> Create([FromBody] CreateTicketDto createTicketDto)
        {
            var result = await _ticketService.CreateTicketAsync(createTicketDto);
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
        [Authorize(Roles = "Admin")]
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
