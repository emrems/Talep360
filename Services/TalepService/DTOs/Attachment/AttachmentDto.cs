namespace TalepService.DTOs.Attachment
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string FilePath { get; set; } = null!;
        public int UploadBy { get; set; }
        public DateTime UploadedAtUtc { get; set; }
    }
}
