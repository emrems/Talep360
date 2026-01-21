namespace TalepService.Entities
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }
}
