using Main.DTOs.UserBasket;

namespace Main.Requests.UserBasket;

public class AddToBasketRequest
{
    public Guid UserId { get; set; }
    public List<BasketItemDTO>? Items { get; set; }
}
