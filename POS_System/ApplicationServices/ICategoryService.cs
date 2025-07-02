using POS_System.Models.Dto;

namespace POS_System.ApplicationServices
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetCategories();
        Task<CategoryDto?> GetCategory(string id);
        Task<CategoryDto> InsertCategory(CreateCategoryRequestDto createCategoryRequestDto);
        Task<CategoryDto?> UpdateCategory(UpdateCategoryRequestDto updateCategoryRequestDto, string id);
        Task<bool> DeleteCategory(string id);
    }
}
