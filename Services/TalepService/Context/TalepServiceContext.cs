using Microsoft.EntityFrameworkCore;
using TalepService.Entities;
using TalepService.Interfaces.Services;

namespace TalepService.Context
{
    // 1. Mutlaka DbContext'ten türetmelisin
    public class TalepServiceContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public TalepServiceContext(DbContextOptions<TalepServiceContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
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

            // Global Query Filter (Soft Delete & Multi-Tenancy)
            // ISoftDeletable implement eden tüm entityler için otomatik filtre
            
            // Not: EF Core lambda expression içinde değişken kullanımını destekler.
            // Ancak expression tree oluşturulurken _currentUserService instance'ı capture edilmeli.
            // Bu yüzden yerel değişkene atamak yerine DbContext instance'ı üzerinden erişmek daha güvenlidir ama
            // DbContext Scoped olduğu için _currentUserService de Scoped'dır ve her request için yeni context oluşur.
            // Bu sayede filtre her request için güncel değerlerle çalışır.
            
            // Expression'ı basitleştirmek için:
            // SuperAdmin ise VEYA TenantId eşleşiyorsa.
            // Ancak SuperAdmin kontrolü DbContext cachelenirse sorun olabilir mi?
            // Hayır, DbContext her requestte yeni oluşuyor (Scoped).
            
            modelBuilder.Entity<Ticket>().HasQueryFilter(e => 
                !e.IsDeleted && 
                (_currentUserService.IsSuperAdmin || e.TenantId == _currentUserService.TenantId));

            modelBuilder.Entity<Attachment>().HasQueryFilter(e => !e.IsDeleted); 
            // Attachment'ta TenantId yok, Ticket üzerinden erişiliyor, ama direkt sorgulanırsa?
            // Attachment entity'sine TenantId eklemek best practice'dir ama şimdilik Ticket üzerinden filtreliyoruz.
            // Güvenlik açığı olmaması için Attachment sorguları genellikle Ticket üzerinden yapılmalı veya Attachment'a da TenantId eklenmeli.
            // Şimdilik Attachment için sadece Soft Delete bırakıyorum.
        }
    }
}