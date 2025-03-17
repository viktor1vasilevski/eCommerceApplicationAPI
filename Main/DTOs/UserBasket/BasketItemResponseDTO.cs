namespace Main.DTOs.UserBasket;

public class BasketItemResponseDTO
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? Edition { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string ImageBase64 { get; set; } = string.Empty;
}

