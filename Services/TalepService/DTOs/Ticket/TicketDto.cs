using TalepService.DTOs.Attachment;
using TalepService.Enums;

namespace TalepService.DTOs.Ticket
{
    public class TicketDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public int CreatedBy { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ClosedAtUtc { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}
