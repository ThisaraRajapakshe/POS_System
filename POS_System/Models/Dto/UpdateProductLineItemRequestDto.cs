namespace POS_System.Models.Dto
{
    public class UpdateProductLineItemRequestDto
    {
        public string BarCodeId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public double Cost { get; set; }
        public double DisplayPrice { get; set; }
        public double DiscountedPrice { get; set; }
        public int Quantity { get; set; }

    }
}
