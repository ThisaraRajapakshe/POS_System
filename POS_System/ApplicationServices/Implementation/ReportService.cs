using NodaTime;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;
using POS_System.Repositories.Implementation;

namespace POS_System.ApplicationServices.Implementation
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly TimeZoneHelper _timeZoneHelper;

        public ReportService(IReportRepository reportRepository, TimeZoneHelper timeZoneHelper)
        {
            _reportRepository = reportRepository;
            _timeZoneHelper = timeZoneHelper;
        }

        public async Task<DailyReportDto> GetDailyReportAsync(LocalDate localDate, string timeZoneId)
        {
            var (utcStart, utcEnd) = _timeZoneHelper.GetUtcRange(localDate, timeZoneId);

            var items = await _reportRepository.GetOrderItemsForUtcRangeAsync(utcStart, utcEnd);
            int totalOrders = await _reportRepository.GetDistinctOrderCountAsync(utcStart, utcEnd);

            var summary = BuildSummary(items, totalOrders);
            var topItems = BuildTopItems(items);

            return new DailyReportDto
            {
                Date = localDate.ToDateTimeUnspecified(),  // just the date part
                Summary = summary,
                TopSellingItems = topItems
            };
        }

        public async Task<ReportSummaryDto> GetMonthlyReportAsync(int year, int month, string timeZoneId)
        {
            LocalDate firstDay = new LocalDate(year, month, 1);
            LocalDate lastDay = firstDay.PlusMonths(1).PlusDays(-1);
            var (utcStart, _) = _timeZoneHelper.GetUtcRange(firstDay, timeZoneId);
            var (_, utcEnd) = _timeZoneHelper.GetUtcRange(lastDay.PlusDays(1), timeZoneId);

            var items = await _reportRepository.GetOrderItemsForUtcRangeAsync(utcStart, utcEnd);
            int totalOrders = await _reportRepository.GetDistinctOrderCountAsync(utcStart, utcEnd);
            return BuildSummary(items, totalOrders);
        }

        public async Task<List<DailyReportDto>> GetWeeklyReportAsync(LocalDate weekStart, string timeZoneId)
        {
            var zone = DateTimeZoneProviders.Tzdb[timeZoneId];

            // UTC start of the first day (Monday 00:00 local)
            var (weekUtcStart, _) = _timeZoneHelper.GetUtcRange(weekStart, timeZoneId);
            // UTC start of the day after the last day (next Monday 00:00) → exclusive end
            var (_, weekUtcEnd) = _timeZoneHelper.GetUtcRange(weekStart.PlusDays(7), timeZoneId);

            var items = await _reportRepository.GetOrderItemsForUtcRangeAsync(weekUtcStart, weekUtcEnd);

            var grouped = items.GroupBy(item =>
            {
                var dto = new DateTimeOffset(item.Order.OrderDate, TimeSpan.Zero); // treat as UTC
                var instant = Instant.FromDateTimeOffset(dto);
                return instant.InZone(zone).Date;
            }).OrderBy(g => g.Key);

            var dailyReports = new List<DailyReportDto>();
            foreach (var group in grouped)
            {
                var dayItems = group.ToList();
                var dayOrders = dayItems.Select(i => i.OrderId).Distinct().Count();
                dailyReports.Add(new DailyReportDto
                {
                    Date = group.Key.ToDateTimeUnspecified(),
                    Summary = BuildSummary(dayItems, dayOrders),
                    TopSellingItems = BuildTopItems(dayItems)
                });
            }
            return dailyReports;
        }
        // Private helpers (stateless, pure functions)
        private ReportSummaryDto BuildSummary(List<OrderItem> items, int totalOrders)
        {
            decimal totalSales = items.Sum(i => i.SubTotal);
            decimal totalCost = items.Sum(i => i.Cost * i.Quantity);
            int totalItems = items.Sum(i => i.Quantity);

            return new ReportSummaryDto
            {
                TotalOrders = totalOrders,
                TotalSales = totalSales,
                TotalCost = totalCost,
                GrossProfit = totalSales - totalCost,
                TotalItemsSold = totalItems,
                AverageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0
            };
        }

        private List<TopSellingItemDto> BuildTopItems(List<OrderItem> items)
        {
            return items
                .GroupBy(i => i.ProductName)
                .Select(g => new TopSellingItemDto
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.SubTotal),
                    Profit = g.Sum(i => (i.SalesPrice - i.Cost) * i.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToList();
        }

        public async Task<MonthlyReportDto> GetMonthlyReportWithDailyAsync(int year, int month, string timeZoneId)
        {
            var zone = DateTimeZoneProviders.Tzdb[timeZoneId];
            LocalDate firstDay = new LocalDate(year, month, 1);
            LocalDate lastDay = firstDay.PlusMonths(1).PlusDays(-1);

            // UTC range for the whole month (from start of first day to start of first day of next month)
            var (monthUtcStart, _) = _timeZoneHelper.GetUtcRange(firstDay, timeZoneId);
            var (_, monthUtcEnd) = _timeZoneHelper.GetUtcRange(lastDay.PlusDays(1), timeZoneId);

            var items = await _reportRepository.GetOrderItemsForUtcRangeAsync(monthUtcStart, monthUtcEnd);
            int totalOrders = await _reportRepository.GetDistinctOrderCountAsync(monthUtcStart, monthUtcEnd);

            // Group all items by local date (just like the weekly method)
            var grouped = items.GroupBy(item =>
            {
                var instant = Instant.FromDateTimeUtc(item.Order.OrderDate);
                return instant.InZone(zone).Date;
            }).OrderBy(g => g.Key);

            var dailyReports = new List<DailyReportDto>();
            foreach (var group in grouped)
            {
                var dayItems = group.ToList();
                var dayOrders = dayItems.Select(i => i.OrderId).Distinct().Count();
                dailyReports.Add(new DailyReportDto
                {
                    Date = group.Key.ToDateTimeUnspecified(),
                    Summary = BuildSummary(dayItems, dayOrders),
                    TopSellingItems = BuildTopItems(dayItems)
                });
            }

            // Build the full monthly summary (using all items)
            var monthlySummary = BuildSummary(items, totalOrders);

            return new MonthlyReportDto
            {
                Year = year,
                Month = month,
                MonthlySummary = monthlySummary,
                DailyReports = dailyReports
            };
        }
    }
}
