using Main.DTOs.UserBasket;

namespace Main.Requests.UserBasket;

public class AddToBasketRequest
{
    public List<BasketItemDTO> Items { get; set; }
}
