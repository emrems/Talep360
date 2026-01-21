using TalepService.Enums;

namespace TalepService.DTOs.Ticket
{
    public class CreateTicketDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketPriority Priority { get; set; }
        public int CreatedBy { get; set; } // Bu normalde token'dan alınır ama şimdilik DTO'da olabilir veya serviste set edilir.
        public int TenantId { get; set; }
    }
}
