using POS_System.Models.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models.Dto
{
    public class CreateOrderDto
    {
        public decimal TotalAmount { get; set; }
        [Required]
        public string PaymentMethod { get; set; } // Cash, Card
        public bool IsPending { get; set; } = false;
        // Navigaion Properties
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
