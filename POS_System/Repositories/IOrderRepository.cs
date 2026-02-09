using POS_System.Models.Domain;

namespace POS_System.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> FindByIdAsync(string id);
    }
}
