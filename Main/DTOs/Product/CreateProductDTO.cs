namespace Main.DTOs.Product;

public class CreateProductDTO
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? UnitQuantity { get; set; }
    public int? Volume { get; set; }
    public string? Scent { get; set; }
    public string? Edition { get; set; }
    public string? Image { get; set; }
}
