using TalepService.DTOs.Ticket;
using TalepService.Wrappers;

namespace TalepService.Interfaces.Services
{
    public interface ITicketService
    {
        Task<BaseResponse<TicketDto>> CreateTicketAsync(CreateTicketDto createTicketDto);
        Task<BaseResponse<TicketDto>> UpdateTicketAsync(UpdateTicketDto updateTicketDto);
        Task<BaseResponse<TicketDto>> GetTicketByIdAsync(int id);
        Task<BaseResponse<IEnumerable<TicketDto>>> GetAllTicketsAsync();
        Task<BaseResponse<IEnumerable<TicketDto>>> GetTicketsByTenantIdAsync(int tenantId);
        Task<BaseResponse<IEnumerable<TicketDto>>> GetTicketsByCreatedByAsync(int userId);
        Task<BaseResponse<IEnumerable<TicketDto>>> GetTicketsByAssignedToAsync(int userId);
        Task<BaseResponse<bool>> DeleteTicketAsync(int id);
        Task<BaseResponse<List<UserWorkloadDto>>> GetWorkloadStatsAsync(List<int> userIds);
        Task<BaseResponse<bool>> RejectTicketAsync(int id, string reason);
        Task<BaseResponse<bool>> AssignTicketAsync(int id, int userId);
        Task<BaseResponse<bool>> AcceptAssignmentAsync(int id);
        // Task<BaseResponse<bool>> RejectAssignmentAsync(int id); // Deprecated
        
        Task<BaseResponse<bool>> ResolveTicketAsync(int id);
        Task<BaseResponse<bool>> CloseTicketAsync(int id);
        Task<BaseResponse<bool>> RequestCustomerInfoAsync(int id, string details);
        Task<BaseResponse<bool>> ReplyToTicketAsync(int id, string message);
    }
}
