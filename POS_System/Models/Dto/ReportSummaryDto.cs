namespace POS_System.Models.Dto
{
    public class ReportSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
