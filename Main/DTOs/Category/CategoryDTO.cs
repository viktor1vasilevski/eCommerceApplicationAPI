namespace Main.DTOs.Category;

public class CategoryDTO
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public List<string>? Subcategories { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? Created { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTime? LastModified { get; set; }
}
