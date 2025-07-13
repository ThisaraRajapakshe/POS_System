namespace POS_System.Models.Dto
{
    public class ProductLineItemDto
    {
        public string Id { get; set; }
        public string BarCodeId { get; set; }
        public string ProductId { get; set; }
        public double Cost { get; set; }
        public double DisplayPrice { get; set; }
        public double DiscountedPrice { get; set; }

        public ProductDto Product { get; set; }
    }
}
