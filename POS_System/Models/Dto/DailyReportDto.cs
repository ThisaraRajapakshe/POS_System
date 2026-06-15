namespace POS_System.Models.Dto
{
    public class DailyReportDto
    {
        public DateTime Date { get; set; }          // local date
        public ReportSummaryDto Summary { get; set; }
        public List<TopSellingItemDto> TopSellingItems { get; set; }
    }
}
