using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models.Domain
{
    public class ProductLineItem
    {
        public string Id { get; set; }
        public string BarCodeId { get; set; }
        public string ProductId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DisplayPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountedPrice { get; set; }
        public int Quantity { get; set; }

        //Navigation Properties
        public Product Product { get; set; }
    }
}
