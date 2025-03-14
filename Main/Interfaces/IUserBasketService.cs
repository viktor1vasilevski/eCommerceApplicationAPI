using Main.DTOs.UserBasket;
using Main.Responses;

namespace Main.Interfaces;

public interface IUserBasketService
{
    Task<ApiResponse<UserBasketItemsDTO>> GetUserBasket();
}
