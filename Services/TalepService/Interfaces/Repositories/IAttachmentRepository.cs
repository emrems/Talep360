using TalepService.Entities;

namespace TalepService.Interfaces.Repositories
{
    public interface IAttachmentRepository : IRepository<Attachment>
    {
        Task<IEnumerable<Attachment>> GetByTicketIdAsync(int ticketId);
    }
}
