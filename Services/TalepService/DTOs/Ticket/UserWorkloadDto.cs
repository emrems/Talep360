namespace TalepService.DTOs.Ticket
{
    public class UserWorkloadDto
    {
        public int UserId { get; set; }
        public int ActiveTicketCount { get; set; } // Open, InProgress, vb.
        public DateTime? LastAssignedDateUtc { get; set; }
    }
}
