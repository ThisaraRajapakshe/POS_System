using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class ProductLineItemRepository: BaseRepository<ProductLineItem, string>, IProductLineItemRepository 
    {
        private readonly PosSystemDbContext dbContext;
        private readonly ILogger<ProductLineItemRepository> logger;

        public ProductLineItemRepository(PosSystemDbContext dbContext, ILogger<ProductLineItemRepository> logger) : base(dbContext)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<List<ProductLineItem>> GetAllWithNavPropsAsync()
        {
            try
            {
                return await dbContext.ProductLineItems
                    .Include(pli => pli.Product)
                        .ThenInclude(p => p.Category)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all product line items with navigation properties");
                throw;
            }
        }

        public async Task<List<ProductLineItem>> GetLineItemByProduct(string productId)
        {
            try
            {
                return await dbContext.ProductLineItems
                    .Where(x => x.ProductId == productId)
                    .Include(pli => pli.Product)
                        .ThenInclude(p => p.Category)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching product line items for product {ProductId}", productId);
                throw;
            }
        }
    }
}
