namespace Main.DTOs.Product;

public class ProductDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Description { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? UnitQuantity { get; set; }
    public int? Volume { get; set; }
    public string? Scent { get; set; }
    public string? Edition { get; set; }
    public byte[] ImageData { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ImageType { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }
    public Guid SubcategoryId { get; set; }
    public virtual string CreatedBy { get; set; }
    public virtual DateTime Created { get; set; }
    public virtual string? LastModifiedBy { get; set; }
    public virtual DateTime? LastModified { get; set; }
}
