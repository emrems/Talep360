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
        Task<BaseResponse<bool>> DeleteTicketAsync(int id);
        Task<BaseResponse<List<UserWorkloadDto>>> GetWorkloadStatsAsync(List<int> userIds);
    }
}
