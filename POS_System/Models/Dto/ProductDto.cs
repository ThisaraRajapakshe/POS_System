using POS_System.Models.Domain;

namespace POS_System.Models.Dto
{
    public class ProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CategoryDto category { get; set; }
    }
}
