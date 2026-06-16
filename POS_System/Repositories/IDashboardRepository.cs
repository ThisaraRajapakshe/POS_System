namespace POS_System.Repositories
{
    public interface IDashboardRepository
    {
        Task<decimal> GetTotalRevenue();
    }
}
