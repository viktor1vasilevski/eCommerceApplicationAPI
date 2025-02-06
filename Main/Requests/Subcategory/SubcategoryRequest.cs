namespace Main.Requests.Subcategory;

public class SubcategoryRequest : BaseRequest
{
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
}
