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

        public async Task<List<Product>> GetProductsByCategoryAsync(string categoryId )
        {
            return await dbContext.Products
                .Where(x => x.CategoryId == categoryId)
                .Include(p => p.Category)
                .ToListAsync();
        }
    }
    
}
