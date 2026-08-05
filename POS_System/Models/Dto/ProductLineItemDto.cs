namespace POS_System.Models.Dto
{
    public class ProductLineItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string BarCodeId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal DisplayPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int Quantity { get; set; }


        public ProductDto Product { get; set; } = new ProductDto();
    }
}
