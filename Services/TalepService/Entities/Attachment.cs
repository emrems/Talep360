namespace TalepService.Entities
{
    public class Attachment : ISoftDeletable
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FilePath { get; set; } = null!;
        public int UploadBy { get; set; }
        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        public Ticket Ticket { get; set; } = null!;
    }
}
