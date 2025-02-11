using Main.DTOs.Category;
using Main.Requests.Category;
using Main.Responses;

namespace Main.Interfaces;

public interface ICategoryService
{
    ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request);
    ApiResponse<CreateCategoryDTO> CreateCategory(CreateEditCategoryRequest request);
    ApiResponse<EditCategoryDTO> EditCategory(Guid id, CreateEditCategoryRequest request);
    NonGenericApiResponse DeleteCategory(Guid id);
    ApiResponse<EditCategoryDTO> GetCategoryById(Guid id);
    ApiResponse<List<SelectCategoryListItemDTO>> GetCategoriesDropdownList();
}
