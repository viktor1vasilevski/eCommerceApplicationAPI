namespace Main.Requests.Subcategory;

public class CreateEditSubcategoryRequest
{
    public string Name { get; set; }
    public Guid CategoryId { get; set; }
}
