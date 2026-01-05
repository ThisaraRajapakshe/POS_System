using POS_System.Models.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models.Dto
{
    public class OrderItemDto
    {
        [Required]
        public string ProductLineItemId { get; set; }
        public decimal SalesPrice { get; set; }
        [Range(1,1000, ErrorMessage ="Quantity Must be at least 1")]
        public int Quantity { get; set; }
    }
}
