using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class ProductLineItemRepository: BaseRepository<ProductLineItem, string>, IProductLineItemRepository 
    {
        private readonly PosSystemDbContext dbContext;

        public ProductLineItemRepository(PosSystemDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<ProductLineItem>> GetAllWithNavPropsAsync()
        {
            return await dbContext.ProductLineItems.
                Include(pli => pli.Product).
                    ThenInclude(p => p.Category).
                ToListAsync();
        }

        public async Task<List<ProductLineItem>> GetLineItemByProduct(string productId)
        {
            return await dbContext.ProductLineItems
                .Where(x => x.ProductId == productId)
                .Include(p => p.Product)
                .ToListAsync();
        }
    }
}
