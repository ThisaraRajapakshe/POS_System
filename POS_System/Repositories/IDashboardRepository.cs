namespace POS_System.Repositories
{
    public interface IDashboardRepository
    {
        Task<decimal> GetTotalRevenue();
        Task<int> GetOrdersToday();
        Task<int> LowStockAlertCount();
    }
}
