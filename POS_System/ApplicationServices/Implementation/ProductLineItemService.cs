using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS_System.ApplicationServices.Implementation
{
    public class ProductLineItemService : IProductLineItemService
    {
        private readonly IProductLineItemRepository repository;
        private readonly IMapper mapper;
        private readonly ILogger<ProductLineItemService> logger;

        public ProductLineItemService(IProductLineItemRepository repository, IMapper mapper, ILogger<ProductLineItemService>? logger = null)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.logger = logger ?? NullLogger<ProductLineItemService>.Instance;
        }

        public Task<bool> DeleteProductLineItem(string id)
        {
            try
            {
                return repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting product line item {Id}", id);
                throw;
            }
        }

        public async Task<List<ProductLineItemDto?>> GetLineItemByProductIdAsync(string productId)
        {
            try
            {
                var domainModel = await repository.GetLineItemByProduct(productId);
                if (domainModel == null)
                {
                    return new List<ProductLineItemDto?>();
                }
                return mapper.Map<List<ProductLineItemDto?>>(domainModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting line items by product {ProductId}", productId);
                throw;
            }
        }

        public async Task<ProductLineItemDto?> GetProductLineItem(string id)
        {
            try
            {
                var domainModel = await repository.GetAsync(id);
                if (domainModel == null)
                {
                    return null;
                }
                return mapper.Map<ProductLineItemDto>(domainModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting product line item {Id}", id);
                throw;
            }
        }

        public async Task<List<ProductLineItemDto>> GetProductLineItems()
        {
            try
            {
                var domainModel = await repository.GetAllWithNavPropsAsync();
                return mapper.Map<List<ProductLineItemDto>>(domainModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting product line items");
                throw;
            }
        }

        public async Task<ProductLineItemDto> InsertProductLineItem(CreateProductLineItemRequestDto productLineItem)
        {
            try
            {
                var domainModel = ProductLineItem.Create(productLineItem.BarCodeId, productLineItem.ProductId, productLineItem.Cost, productLineItem.DisplayPrice, productLineItem.DiscountedPrice, productLineItem.Quantity);
                domainModel = await repository.CreateAsync(domainModel);
                var productLineItemDto = mapper.Map<ProductLineItemDto>(domainModel);
                return productLineItemDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inserting product line item: {ProductLineItem}", productLineItem);
                throw;
            }
        }

        public async Task<ProductLineItemDto?> UpdateProductLineItem(UpdateProductLineItemRequestDto updateProductLineItemRequestDto, string id)
        {
            try
            {
                var existing = await repository.GetAsync(id);
                if (existing == null)
                {
                    return null;
                }

                // Apply domain updates
                if (!string.IsNullOrWhiteSpace(updateProductLineItemRequestDto.BarCodeId))
                {
                    existing.UpdateBarCode(updateProductLineItemRequestDto.BarCodeId);
                }

                if (!string.IsNullOrWhiteSpace(updateProductLineItemRequestDto.ProductId))
                {
                    existing.UpdateProduct(updateProductLineItemRequestDto.ProductId);
                }

                existing.UpdatePrice(updateProductLineItemRequestDto.Cost, updateProductLineItemRequestDto.DisplayPrice, updateProductLineItemRequestDto.DiscountedPrice);
                existing.UpdateQuantity(updateProductLineItemRequestDto.Quantity);

                var updated = await repository.UpdateAsync(existing, id);
                if (updated == null)
                {
                    return null;
                }
                return mapper.Map<ProductLineItemDto>(updated);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating product line item {Id}", id);
                throw;
            }
        }
    }
}
