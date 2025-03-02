namespace Main.Requests.Product;

public class ProductRequest : BaseRequest
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string? Edition { get; set; }
    public string? Scent { get; set; }
    public string? Price { get; set; }
    public string? Quantity { get; set; }
    public string? Volume { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? SubcategoryId { get; set; }
}
