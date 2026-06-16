namespace POS_System.Models.Dto
{
    public class AdminDashboardDto
    {
        public decimal TotalRevenue { get; set; }
        public int OrdersToday { get; set; }
        public int LowStockAlerts { get; set; }
    }
}
