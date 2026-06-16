using Microsoft.EntityFrameworkCore;
using POS_System.Data;

namespace POS_System.Repositories.Implementation
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly PosSystemDbContext _context;
        public DashboardRepository(PosSystemDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetOrdersToday()
        {
            return await _context.Orders
                .Where(o => o.OrderDate.Date == DateTime.UtcNow.Date)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenue()
        {
            // Sum the TotalAmount of all completed orders (revenue)
            return await _context.Orders
                .Where(o => o.PaymentMethod != "Credit")   // optional: only paid orders
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> LowStockAlertCount()
        {
            return await _context.ProductLineItems
                .Where(p => p.Quantity < 10)
                .CountAsync();
        }
    }
}
