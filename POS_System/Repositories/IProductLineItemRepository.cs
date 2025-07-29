using POS_System.Models.Domain;

namespace POS_System.Repositories
{
    public interface IProductLineItemRepository: IBaseRepository<ProductLineItem, string>
    {
        Task<List<ProductLineItem>> GetAllWithNavPropsAsync();
        Task<List<ProductLineItem>> GetLineItemByProduct(string productId);
    }
}
