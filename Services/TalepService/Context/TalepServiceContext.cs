using Microsoft.EntityFrameworkCore;
using TalepService.Entities;

namespace TalepService.Context
{
    // 1. Mutlaka DbContext'ten türetmelisin
    public class TalepServiceContext : DbContext
    {
       
        public TalepServiceContext(DbContextOptions<TalepServiceContext> options) : base(options)
        {
        }

      
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ticket Tablosu Konfigürasyonu
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();

                // Bir Ticket'ın birçok Attachment'ı olabilir (1-N ilişki)
                entity.HasMany(e => e.Attachments)
                      .WithOne(a => a.Ticket)
                      .HasForeignKey(a => a.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Attachment Tablosu Konfigürasyonu
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired();
            });
        }
    }
}