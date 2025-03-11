using Main.DTOs.Subcategory;

namespace Main.DTOs.Category;

public class CategoryWithSubcategoriesDetialsDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public List<SubcategorySlugDTO> Subcategories { get; set; }
}
