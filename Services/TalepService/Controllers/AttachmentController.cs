using Microsoft.AspNetCore.Mvc;
using TalepService.DTOs.Attachment;
using TalepService.Interfaces.Services;
using TalepService.Wrappers;

namespace TalepService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;

        public AttachmentController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<BaseResponse<IEnumerable<AttachmentDto>>>> GetByTicketId(int ticketId)
        {
            var result = await _attachmentService.GetAttachmentsByTicketIdAsync(ticketId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<AttachmentDto>>> Create([FromBody] CreateAttachmentDto createAttachmentDto)
        {
            var result = await _attachmentService.AddAttachmentAsync(createAttachmentDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(int id)
        {
            var result = await _attachmentService.DeleteAttachmentAsync(id);
            return Ok(result);
        }
    }
}
