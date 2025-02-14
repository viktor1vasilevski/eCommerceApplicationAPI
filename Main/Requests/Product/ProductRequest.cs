namespace Main.Requests.Product;

public class ProductRequest : BaseRequest
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Edition { get; set; }
    public string? Scent { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SubcategoryId { get; set; }
}
