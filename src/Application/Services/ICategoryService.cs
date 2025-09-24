using Application.DTOs.Category;
using Application.DTOs.Common;

namespace Application.Services;

public interface ICategoryService
{
    Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<CategoryDto>> GetBySlugAsync(string slug);
    Task<ApiResponse<List<CategoryDto>>> GetAllAsync();
    Task<ApiResponse<List<CategoryDto>>> GetRootCategoriesAsync();
    Task<ApiResponse<List<CategoryDto>>> GetSubCategoriesAsync(Guid parentId);
    Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto createCategoryDto);
    Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryDto updateCategoryDto);
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
    Task<ApiResponse<bool>> ToggleActiveAsync(Guid id);
}