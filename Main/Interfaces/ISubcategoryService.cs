using Main.DTOs.Subcategory;
using Main.Requests.Subcategory;
using Main.Responses;

namespace Main.Interfaces;

public interface ISubcategoryService
{
    ApiResponse<List<SubcategoryDTO>> GetSubcategories(SubcategoryRequest request);
    ApiResponse<CreateSubcategoryDTO> CreateSubcategory(CreateSubcategoryRequest request);
}
