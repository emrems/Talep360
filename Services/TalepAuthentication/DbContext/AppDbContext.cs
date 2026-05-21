using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalepAuthentication.Entities;

namespace TalepAuthentication.DbContext
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // seed data ile rolleri verdik
            builder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int>
                {
                    Id = 1,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole<int>
                {
                    Id = 2,
                    Name = "Manager",
                    NormalizedName = "MANAGER"
                },
                new IdentityRole<int>
                {
                    Id = 3,
                    Name = "User",
                    NormalizedName = "USER"
                },
                new IdentityRole<int>
                {
                    Id = 5,
                    Name = "Staff",
                    NormalizedName = "STAFF"
                },
                new IdentityRole<int>
                {
                    Id = 99,
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN"
                }
            );

            // Default Tenant (Sistem Sahibi)
            builder.Entity<Tenant>().HasData(
                new Tenant { Id = 1, Name = "Talep360 Host", IsActive = true }
            );

            // Seed Admin User
            var adminUser = new User
            {
                Id = 4,
                UserName = "admin@talep360.com",
                NormalizedUserName = "ADMIN@TALEP360.COM",
                Email = "admin@talep360.com",
                NormalizedEmail = "ADMIN@TALEP360.COM",
                EmailConfirmed = true,
                FullName = "System Admin",
                TenantId = 1,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var passwordHasher = new PasswordHasher<User>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin123!");

            builder.Entity<User>().HasData(adminUser);

            // Assign Admin Role to Admin User
            builder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int>
                {
                    RoleId = 1, // Admin Role
                    UserId = 4
                }
            );

            // Seed SuperAdmin User
            var superAdminUser = new User
            {
                Id = 99,
                UserName = "superadmin@talep360.com",
                NormalizedUserName = "SUPERADMIN@TALEP360.COM",
                Email = "superadmin@talep360.com",
                NormalizedEmail = "SUPERADMIN@TALEP360.COM",
                EmailConfirmed = true,
                FullName = "Super Administrator",
                TenantId = 1,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            superAdminUser.PasswordHash = passwordHasher.HashPassword(superAdminUser, "SuperAdmin123!");

            builder.Entity<User>().HasData(superAdminUser);

            // Assign SuperAdmin Role to SuperAdmin User
            builder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int>
                {
                    RoleId = 99, // SuperAdmin Role
                    UserId = 99
                }
            );
        }
    }
}
