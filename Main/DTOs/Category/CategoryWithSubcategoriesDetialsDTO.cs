using Main.DTOs.Subcategory;

namespace Main.DTOs.Category;

public class CategoryWithSubcategoriesDetialsDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<SubcategoryDetailsDTO> Subcategories { get; set; }
}
