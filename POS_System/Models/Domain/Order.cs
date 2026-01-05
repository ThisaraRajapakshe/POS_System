using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models.Domain
{
    public class Order
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } // Cash, Card
        public string UserId { get; set; }
        public string CashierName { get; set; }
        public string Status { get; set; } // pending , Completed
        // Navigaion Properties
        public List<OrderItem> OrderItems { get; set; }
    }
}
