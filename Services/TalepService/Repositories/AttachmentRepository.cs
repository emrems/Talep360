using Microsoft.EntityFrameworkCore;
using TalepService.Context;
using TalepService.Entities;
using TalepService.Interfaces.Repositories;

namespace TalepService.Repositories
{
    public class AttachmentRepository : Repository<Attachment>, IAttachmentRepository
    {
        public AttachmentRepository(TalepServiceContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Attachment>> GetByTicketIdAsync(int ticketId)
        {
            return await _context.Attachments
                .Where(a => a.TicketId == ticketId)
                .ToListAsync();
        }
    }
}
