using POS_System.Models.Dto;

namespace POS_System.ApplicationServices
{
    public interface IProductLineItemService
    {
        Task<List<ProductLineItemDto>> GetProductLineItems();
        Task<ProductLineItemDto?> GetProductLineItem(string id);
        Task<ProductLineItemDto> InsertProductLineItem(CreateProductLineItemRequestDto productLineItem);
        Task<ProductLineItemDto?> UpdateProductLineItem(UpdateProductLineItemRequestDto updateProductLineItemRequestDto, string id);
        Task<bool> DeleteProductLineItem(string id);
        Task<List<ProductLineItemDto?>> GetLineItemByProductIdAsync(string productId);
    }
}
