using AutoMapper;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;
using POS_System.Repositories.Implementation;

namespace POS_System.ApplicationServices.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IMapper mapper;
        private readonly ICategoryRepository repository;

        public CategoryService(IMapper mapper, ICategoryRepository repository)
        {
            this.mapper = mapper;
            this.repository = repository;
        }
        public Task<bool> DeleteCategory(string id)
        {
            return repository.DeleteAsync(id);

        }

        public async Task<List<CategoryDto>> GetCategories()
        {
            var domainModel = await repository.GetAsync();
            return mapper.Map<List<CategoryDto>>(domainModel);
        }

        public async Task<CategoryDto?> GetCategory(string id)
        {
            var domainModel = await repository.GetAsync(id);
            if (domainModel == null)
            {
                return null;
            }
            return mapper.Map<CategoryDto>(domainModel);
        }

        public async Task<CategoryDto> InsertCategory(CreateCategoryRequestDto createCategoryRequestDto)
        {
            var domainModel = mapper.Map<Category>(createCategoryRequestDto);
            domainModel = await repository.CreateAsync(domainModel);
            var categoryDto = mapper.Map<CategoryDto>(domainModel);
            return categoryDto;
        }

        public async Task<CategoryDto?> UpdateCategory(UpdateCategoryRequestDto updateCategoryRequestDto, string id)
        {
            var domainModel = mapper.Map<Category>(updateCategoryRequestDto);
            domainModel = await repository.UpdateAsync(domainModel, id);
            if (domainModel == null)
            {
                return null;
            }
            var categoryDto = mapper.Map<CategoryDto>(domainModel);
            return categoryDto;
        }
    }
}
