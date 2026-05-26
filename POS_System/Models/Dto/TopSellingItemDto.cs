namespace POS_System.Models.Dto
{
    public class TopSellingItemDto
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Profit { get; set; }
    }
}
