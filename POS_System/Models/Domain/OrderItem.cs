using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models.Domain
{
    public class OrderItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OderId { get; set; }
        public Order Order { get; set; }

        public string ProductLineItemId { get; set; }
        //SnapShot
        public string ProductName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DisplayPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalesPrice { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

    }
}
