using POS_System.Models.Domain;

namespace POS_System.Repositories
{
    public interface IProductRepository: IBaseRepository<Product, string>
    {
        Task<List<Product>> GetAllWithCategoryAsync();
        Task<List<Product>> GetProductsByCategoryAsync(string categoryId);
    }
}
