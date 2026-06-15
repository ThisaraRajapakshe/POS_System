using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class ReportRepository : IReportRepository
    {
        private readonly PosSystemDbContext _context;

        public ReportRepository(PosSystemDbContext context)
        {
            this._context = context;
        }

        public async Task<int> GetDistinctOrderCountAsync(DateTime utcStart, DateTime utcEnd)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= utcStart && o.OrderDate < utcEnd)
                .Select(o => o.Id)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<OrderItem>> GetOrderItemsForUtcRangeAsync(DateTime utcStart, DateTime utcEnd)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)            // needed? we only need Order.OrderDate and Order.Id
                .Where(oi => oi.Order.OrderDate >= utcStart && oi.Order.OrderDate < utcEnd)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
