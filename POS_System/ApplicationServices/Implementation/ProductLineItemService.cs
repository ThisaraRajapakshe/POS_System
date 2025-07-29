using AutoMapper;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS_System.ApplicationServices.Implementation
{
    public class ProductLineItemService : IProductLineItemService
    {
        private readonly IProductLineItemRepository repository;
        private readonly IMapper mapper;

        public ProductLineItemService(IProductLineItemRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }
        public Task<bool> DeleteProductLineItem(string id)
        {
            return repository.DeleteAsync(id);
        }

        public async Task<List<ProductLineItemDto?>> GetLineItemByProductIdAsync(string productId)
        {
            var domainModel = await repository.GetLineItemByProduct(productId);
            if (domainModel == null) 
            {
                return new List<ProductLineItemDto?>();
            }
            return mapper.Map<List<ProductLineItemDto?>>(domainModel);
        }

        public async Task<ProductLineItemDto?> GetProductLineItem(string id)
        {
            var domainModel = await repository.GetAsync(id);
            return mapper.Map<ProductLineItemDto>(domainModel);
        }

        public async Task<List<ProductLineItemDto>> GetProductLineItems()
        {
            var domainModel = await repository.GetAllWithNavPropsAsync();
            return mapper.Map<List<ProductLineItemDto>>(domainModel);
        }

        public async Task<ProductLineItemDto> InsertProductLineItem(CreateProductLineItemRequestDto productLineItem)
        {
            var domainModel = mapper.Map<ProductLineItem>(productLineItem);
            domainModel = await repository.CreateAsync(domainModel);
            var productLineItemDto = mapper.Map<ProductLineItemDto>(domainModel);
            return productLineItemDto;
        }

        public async Task<ProductLineItemDto?> UpdateProductLineItem(UpdateProductLineItemRequestDto updateProductLineItemRequestDto, string id)
        {
            var domainModel = mapper.Map<ProductLineItem>(updateProductLineItemRequestDto);
            domainModel = await repository.UpdateAsync(domainModel, id);
            if (domainModel == null)
            {
                return null;
            }
            return mapper.Map<ProductLineItemDto>(domainModel);
            
        }
    }
}
