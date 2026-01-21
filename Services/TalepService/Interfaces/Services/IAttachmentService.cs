using TalepService.DTOs.Attachment;
using TalepService.Wrappers;

namespace TalepService.Interfaces.Services
{
    public interface IAttachmentService
    {
        Task<BaseResponse<AttachmentDto>> AddAttachmentAsync(CreateAttachmentDto createAttachmentDto);
        Task<BaseResponse<bool>> DeleteAttachmentAsync(int id);
        Task<BaseResponse<IEnumerable<AttachmentDto>>> GetAttachmentsByTicketIdAsync(int ticketId);
    }
}
