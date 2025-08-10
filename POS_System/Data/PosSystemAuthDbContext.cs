using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS_System.Models.Identity;

namespace POS_System.Data
{
    public class PosSystemAuthDbContext : IdentityDbContext
    {
        public PosSystemAuthDbContext(DbContextOptions<PosSystemAuthDbContext> options) : base(options)
        {
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    }
}

