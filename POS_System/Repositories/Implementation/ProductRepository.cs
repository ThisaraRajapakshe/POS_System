using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class ProductRepository : BaseRepository<Product, string>, IProductRepository
    {
        public ProductRepository(PosSystemDbContext dbContext) : base(dbContext)
        {
        }
    }
}
