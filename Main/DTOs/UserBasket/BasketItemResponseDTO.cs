namespace Main.DTOs.UserBasket;

public class BasketItemResponseDTO
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductBrand { get; set; } = string.Empty;
    public string ProductEdition { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageBase64 { get; set; } = string.Empty;
}

