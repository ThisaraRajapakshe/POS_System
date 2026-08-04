namespace POS_System.Models.Dto
{
    public class CreateProductLineItemRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public string BarCodeId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal DisplayPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int Quantity { get; set; }

    }
}
