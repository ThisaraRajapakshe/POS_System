namespace POS_System.Models.Dto
{
    public class CreateProductRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
    }
}
