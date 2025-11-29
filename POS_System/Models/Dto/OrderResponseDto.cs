namespace POS_System.Models.Dto
{
    public class OrderResponseDto
    {
        public string Id { get; set; } // The Order Guid
        public string OrderNumber { get; set; } // "INV-2025-001"
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; } // "Completed", "Pending"

        // Audit Info
        public string CashierName { get; set; }

        // The List (Using the DTO, NOT the Entity)
        public List<OrderItemResponseDto> OrderItems { get; set; }
    }
}