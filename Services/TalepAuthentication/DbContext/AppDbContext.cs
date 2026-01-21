using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalepAuthentication.Entities;

namespace TalepAuthentication.DbContext
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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
                }
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
                    RoleId = 1, 
                    UserId = 4  
                }
            );
        }
    }
}
