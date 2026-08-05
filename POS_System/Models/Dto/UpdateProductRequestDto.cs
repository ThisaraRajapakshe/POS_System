using System.ComponentModel.DataAnnotations;

namespace POS_System.Models.Dto
{
    public class UpdateProductRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
    }
}
