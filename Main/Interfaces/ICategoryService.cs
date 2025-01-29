using Main.DTOs.Category;
using Main.Requests.Category;
using Main.Responses;

namespace Main.Interfaces;

public interface ICategoryService
{
    ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request);
    ApiResponse<CreateCategoryDTO> CreateCategory(CreateCategoryRequest request);
}
