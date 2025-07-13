using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class ProductRepository : BaseRepository<Product, string>, IProductRepository
    {
        private readonly PosSystemDbContext dbContext;

        public ProductRepository(PosSystemDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public  async Task<List<Product>> GetAllWithCategoryAsync()
        {
            return await dbContext.Products
                .Include(p => p.Category)
                .ToListAsync();
        }
    }
}
