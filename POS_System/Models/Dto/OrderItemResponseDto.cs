namespace POS_System.Models.Dto
{
    public class OrderItemResponseDto
    {
        public string Id { get; set; } // The ID of this specific line item
        public string ProductLineItemId { get; set; } // The Inventory ID

        // Snapshot Data (Read-Only)
        public string ProductName { get; set; }
        public decimal DisplayPrice { get; set; }
        public decimal SalesPrice { get; set; } // What they actually paid
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }
}
