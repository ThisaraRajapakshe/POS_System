using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class ProductLineItemRepository: BaseRepository<ProductLineItem, string>, IProductLineItemRepository 
    {
        public ProductLineItemRepository(PosSystemDbContext dbContext) : base(dbContext)
        {
            
        }
    }
}
