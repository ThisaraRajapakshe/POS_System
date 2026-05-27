using NodaTime;
using POS_System.Models.Dto;

namespace POS_System.ApplicationServices
{
    public interface IReportService
    {
        Task<DailyReportDto> GetDailyReportAsync(LocalDate localDate, string timeZoneId);
        Task<List<DailyReportDto>> GetWeeklyReportAsync(LocalDate weekStart, string timeZoneId);
        Task<ReportSummaryDto> GetMonthlyReportAsync(int year, int month, string timeZoneId);
    }
}
