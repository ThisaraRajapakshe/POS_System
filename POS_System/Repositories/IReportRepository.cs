using POS_System.Models.Domain;

namespace POS_System.Repositories
{
    public interface IReportRepository
    {
        Task<List<OrderItem>> GetOrderItemsForUtcRangeAsync(DateTime utcStart, DateTime utcEnd);
        Task<int> GetDistinctOrderCountAsync(DateTime utcStart, DateTime utcEnd);
    }
}
