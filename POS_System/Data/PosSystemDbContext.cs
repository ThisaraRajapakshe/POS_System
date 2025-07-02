using Microsoft.EntityFrameworkCore;
using POS_System.Models.Domain;

namespace POS_System.Data
{
    public class PosSystemDbContext: DbContext
    {
        public PosSystemDbContext(DbContextOptions dbContextOptions): base(dbContextOptions)
        {
            
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductLineItem> ProductLineItems { get; set; }




    }
}
