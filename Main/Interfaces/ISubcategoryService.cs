using Main.DTOs.Category;
using Main.DTOs.Subcategory;
using Main.Requests.Subcategory;
using Main.Responses;

namespace Main.Interfaces;

public interface ISubcategoryService
{
    ApiResponse<List<SubcategoryDTO>> GetSubcategories(SubcategoryRequest request);
    ApiResponse<CreateSubcategoryDTO> CreateSubcategory(CreateEditSubcategoryRequest request);
    ApiResponse<SubcategoryDetailsDTO> GetSubcategoryById(Guid id);
    ApiResponse<SubcategoryDTO> EditSubcategory(Guid id, CreateEditSubcategoryRequest request);
}
