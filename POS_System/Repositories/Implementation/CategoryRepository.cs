using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class CategoryRepository: BaseRepository<Category, string>, ICategoryRepository 
    {
        public CategoryRepository(PosSystemDbContext dbContext): base(dbContext) 
        {
        
        }
    }
}
