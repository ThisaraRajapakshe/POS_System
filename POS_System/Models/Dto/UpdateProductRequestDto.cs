using System.ComponentModel.DataAnnotations;

namespace POS_System.Models.Dto
{
    public class UpdateProductRequestDto
    {
        public string Name { get; set; }
        public string CategoryId { get; set; }
    }
}
