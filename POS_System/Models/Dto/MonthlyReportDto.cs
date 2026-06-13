namespace POS_System.Models.Dto
{
    public class MonthlyReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public ReportSummaryDto MonthlySummary { get; set; }
        public List<DailyReportDto> DailyReports { get; set; }
    }
}
