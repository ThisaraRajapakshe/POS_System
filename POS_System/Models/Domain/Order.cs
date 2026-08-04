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

        public Order()
        {
        }

        /// <summary>
        /// Initialize order for creation (assign ids, timestamps, numbers and basic status)
        /// </summary>
        public void InitializeForCreate(string userId, string cashierName, bool isPending)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId must be provided", nameof(userId));
            if (string.IsNullOrWhiteSpace(cashierName)) cashierName = "";

            if (string.IsNullOrWhiteSpace(Id))
            {
                Id = Guid.NewGuid().ToString();
            }

            if (OrderItems != null)
            {
                foreach (var item in OrderItems)
                {
                    if (string.IsNullOrWhiteSpace(item.Id)) item.Id = Guid.NewGuid().ToString();
                    item.OrderId = Id;
                }
            }

            UserId = userId;
            CashierName = cashierName;
            OrderDate = DateTime.UtcNow;
            Status = isPending ? "Pending" : "Completed";
            OrderNumber = $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
        }

        public void CalculateTotal()
        {
            if (OrderItems == null) { TotalAmount = 0; return; }
            decimal total = 0;
            foreach (var item in OrderItems)
            {
                total += item.SubTotal;
            }
            TotalAmount = total;
        }
    }
}
