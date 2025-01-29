namespace Main.Requests.Category;

public class CategoryRequest
{
    public string? Name { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public string? Sort { get; set; }
}
