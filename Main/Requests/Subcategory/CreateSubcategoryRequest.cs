namespace Main.Requests.Subcategory;

public class CreateSubcategoryRequest
{
    public string Name { get; set; }
    public Guid CategoryId { get; set; }
}
