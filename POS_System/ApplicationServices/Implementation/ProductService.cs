using AutoMapper;
using Microsoft.Extensions.Logging;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS_System.ApplicationServices.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repository;
        private readonly IMapper mapper;
        private readonly ILogger<ProductService> logger;

        public ProductService(IProductRepository repository, IMapper mapper, ILogger<ProductService> logger)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public Task<bool> DeleteProduct(string id)
        {
            try
            {
                return repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting product with id {ProductId}", id);
                throw;
            }
        }

        public async Task<List<ProductDto>> GetProducts()
        {
            try
            {
                var productDomainModel = await repository.GetAllWithCategoryAsync();
                return mapper.Map<List<ProductDto>>(productDomainModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting products");
                throw;
            }
        }

        public async Task<ProductDto?> GetProduct(string id)
        {
            try
            {
                var productDomainModel = await repository.GetAsync(id);
                if (productDomainModel == null)
                {
                    return null;
                }
                return mapper.Map<ProductDto>(productDomainModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting product with id {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductDto> InsertProduct(CreateProductRequestDto createProductRequestDto)
        {
            try
            {
                // use domain factory to create product and enforce invariants
                var domainModel = Product.Create(createProductRequestDto.Name, createProductRequestDto.CategoryId);
                domainModel = await repository.CreateAsync(domainModel);
                var productDto = mapper.Map<ProductDto>(domainModel);
                return productDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inserting product: {Product}", createProductRequestDto);
                throw;
            }
        }

        public async Task<ProductDto?> UpdateProduct(UpdateProductRequestDto updateProductRequestDto, string id)
        {
            try
            {
                var existing = await repository.GetAsync(id);
                if (existing == null)
                {
                    return null;
                }

                // apply domain operations
                existing.UpdateName(updateProductRequestDto.Name);
                if (!string.IsNullOrWhiteSpace(updateProductRequestDto.CategoryId))
                {
                    existing.UpdateCategory(updateProductRequestDto.CategoryId);
                }

                var updated = await repository.UpdateAsync(existing, id);
                if (updated == null)
                {
                    return null;
                }
                var productDto = mapper.Map<ProductDto>(updated);
                return productDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating product with id {ProductId}", id);
                throw;
            }

        }

        public async Task<List<ProductDto?>> GetProductsByCategory(string categoryId)
        {
            try
            {
                var domainModel = await repository.GetProductsByCategoryAsync(categoryId);
                if (domainModel == null)
                {
                    return new List<ProductDto?>();
                }
                return mapper.Map<List<ProductDto?>>(domainModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting products by category {CategoryId}", categoryId);
                throw;
            }
        }
    }
}
