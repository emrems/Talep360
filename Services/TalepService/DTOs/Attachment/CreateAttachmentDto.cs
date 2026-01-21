namespace TalepService.DTOs.Attachment
{
    public class CreateAttachmentDto
    {
        public int TicketId { get; set; }
        public string FilePath { get; set; } = null!;
        public int UploadBy { get; set; }
    }
}
