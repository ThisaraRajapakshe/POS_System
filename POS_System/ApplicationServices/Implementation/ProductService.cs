using AutoMapper;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS_System.ApplicationServices.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repository;
        private readonly IMapper mapper;

        public ProductService(IProductRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public Task<bool> DeleteProduct(string id)
        {
            return repository.DeleteAsync(id);
        }

        public async Task<List<ProductDto>> GetProducts()
        {
            var productDomainModel = await repository.GetAllWithCategoryAsync();
            return  mapper.Map<List<ProductDto>>(productDomainModel);
        }

        public async Task<ProductDto?> GetProduct(string id)
        {
            var productDomainModel = await repository.GetAsync(id);
            if (productDomainModel == null)
            {
                return null;
            }
            return mapper.Map<ProductDto>(productDomainModel);
        }

        public async Task<ProductDto> InsertProduct(CreateProductRequestDto createProductRequestDto)
        {
            var domainModel = mapper.Map<Product>(createProductRequestDto);
            domainModel = await repository.CreateAsync(domainModel);
            var productDto = mapper.Map<ProductDto>(domainModel);
            return productDto;
        }

        public async Task<ProductDto?> UpdateProduct(UpdateProductRequestDto updateProductRequestDto, string id)
        {
            var domainModel = mapper.Map<Product>(updateProductRequestDto);
            domainModel = await repository.UpdateAsync(domainModel, id);
            if (domainModel == null)
            {
                return null;
            }
            var productDto = mapper.Map<ProductDto>(domainModel);
            return productDto;

        }

        public async Task<List<ProductDto?>> GetProductsByCategory(string categoryId)
        {
            var domainModel = await repository.GetProductsByCategoryAsync(categoryId);
            if (domainModel == null)
            {
                return new List<ProductDto?>();
            }
            return mapper.Map<List<ProductDto?>>(domainModel);
            
        }
    }
}
