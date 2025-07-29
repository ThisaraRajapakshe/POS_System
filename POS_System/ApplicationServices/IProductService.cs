using POS_System.Models.Domain;
using POS_System.Models.Dto;

namespace POS_System.ApplicationServices
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetProducts();
        Task<ProductDto?> GetProduct(string id);
        Task<ProductDto> InsertProduct(CreateProductRequestDto product);
        Task<ProductDto?> UpdateProduct(UpdateProductRequestDto updateProductRequestDto, string id);
        Task<bool> DeleteProduct(string id);
        Task<List<ProductDto?>> GetProductsByCategory(string categoryId);
    }
}
