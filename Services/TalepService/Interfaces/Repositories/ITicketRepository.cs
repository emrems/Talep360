using TalepService.Entities;

namespace TalepService.Interfaces.Repositories
{
    public interface ITicketRepository : IRepository<Ticket>
    {
        Task<Ticket?> GetTicketWithAttachmentsAsync(int id);
        Task<IEnumerable<Ticket>> GetActiveTicketsByUserIdsAsync(List<int> userIds);
        Task<IEnumerable<Ticket>> GetByTenantIdAsync(int tenantId);
        Task<IEnumerable<Ticket>> GetByCreatedByAsync(int userId);
        Task<IEnumerable<Ticket>> GetByAssignedToAsync(int userId);
    }
}
