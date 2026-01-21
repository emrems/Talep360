using TalepService.Enums;

namespace TalepService.DTOs.Ticket
{
    public class UpdateTicketDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TicketPriority? Priority { get; set; }
        public TicketStatus? Status { get; set; }
        public int? AssignedTo { get; set; }
    }
}
