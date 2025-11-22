using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS_System.Models.Identity;

namespace POS_System.Data
{
    public class PosSystemAuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public PosSystemAuthDbContext(DbContextOptions<PosSystemAuthDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure RefreshToken entity
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.JwtId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UserId).IsRequired();

                // Create index for performance
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.JwtId);
                entity.HasIndex(e => e.UserId);
            });

            // Configure ApplicationRole
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Configure ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.EmployeeId).HasMaxLength(50);
                entity.Property(e => e.BranchId).HasMaxLength(50);
                entity.Property(e => e.BranchName).HasMaxLength(100);

                // Create indexes for performance
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.BranchId);
            });
        }
    }
}
