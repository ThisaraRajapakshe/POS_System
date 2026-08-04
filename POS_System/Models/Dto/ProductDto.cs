using POS_System.Models.Domain;

namespace POS_System.Models.Dto
{
    public class ProductDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public CategoryDto category { get; set; } = new CategoryDto();
    }
}
