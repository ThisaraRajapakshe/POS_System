using AutoMapper;
using Microsoft.Extensions.Logging;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS_System.ApplicationServices.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IMapper mapper;
        private readonly ICategoryRepository repository;
        private readonly ILogger<CategoryService> logger;

        public CategoryService(IMapper mapper, ICategoryRepository repository, ILogger<CategoryService> logger)
        {
            this.mapper = mapper;
            this.repository = repository;
            this.logger = logger;
        }

        public Task<bool> DeleteCategory(string id)
        {
            try
            {
                return repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting category with id {CategoryId}", id);
                throw;
            }
        }

        public async Task<List<CategoryDto>> GetCategories()
        {
            try
            {
                var domainModel = await repository.GetAsync();
                return mapper.Map<List<CategoryDto>>(domainModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting categories");
                throw;
            }
        }

        public async Task<CategoryDto?> GetCategory(string id)
        {
            try
            {
                var domainModel = await repository.GetAsync(id);
                if (domainModel == null)
                {
                    return null;
                }
                return mapper.Map<CategoryDto>(domainModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting category with id {CategoryId}", id);
                throw;
            }
        }

        public async Task<CategoryDto> InsertCategory(CreateCategoryRequestDto createCategoryRequestDto)
        {
            try
            {
                var domainModel = Category.Create(createCategoryRequestDto.Name);
                domainModel = await repository.CreateAsync(domainModel);
                var categoryDto = mapper.Map<CategoryDto>(domainModel);
                return categoryDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inserting category: {Category}", createCategoryRequestDto);
                throw;
            }
        }

        public async Task<CategoryDto?> UpdateCategory(UpdateCategoryRequestDto updateCategoryRequestDto, string id)
        {
            try
            {
                var existing = await repository.GetAsync(id);
                if (existing == null)
                {
                    return null;
                }

                existing.UpdateName(updateCategoryRequestDto.Name);

                var updated = await repository.UpdateAsync(existing, id);
                if (updated == null)
                {
                    return null;
                }
                var categoryDto = mapper.Map<CategoryDto>(updated);
                return categoryDto;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating category with id {CategoryId}", id);
                throw;
            }
        }
    }
}
