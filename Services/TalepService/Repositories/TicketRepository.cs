using Microsoft.EntityFrameworkCore;
using TalepService.Context;
using TalepService.Entities;
using TalepService.Enums;
using TalepService.Interfaces.Repositories;

namespace TalepService.Repositories
{
    public class TicketRepository : Repository<Ticket>, ITicketRepository
    {
        public TicketRepository(TalepServiceContext context) : base(context)
        {
        }

        public async Task<Ticket?> GetTicketWithAttachmentsAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Ticket>> GetActiveTicketsByUserIdsAsync(List<int> userIds)
        {
            return await _context.Tickets
                .Where(t => t.AssignedTo.HasValue && 
                            userIds.Contains(t.AssignedTo.Value) &&
                            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
                .ToListAsync();
        }
    }
}
