namespace POS_System.Models.Domain
{
    public class ProductLineItem
    {
        public string Id { get; set; }
        public string BarCodeId { get; set; }
        public string ProductId { get; set; }
        public double Cost { get; set; }
        public double DisplayPrice { get; set; }
        public double DiscountedPrice { get; set; }

        //Navigation Properties
        public Product Product { get; set; }
    }
}
