using TalepService.Enums;

namespace TalepService.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public int TenantId { get; set; } // Çoklu müşteri (Multi-tenant) yapısı için
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public int CreatedBy { get; set; } // IdentityService'den gelecek User Id
        public int? AssignedTo { get; set; } // Atanan personel Id
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAtUtc { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
