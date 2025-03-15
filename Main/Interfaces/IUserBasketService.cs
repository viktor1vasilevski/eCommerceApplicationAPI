using Main.DTOs.UserBasket;
using Main.Requests.UserBasket;
using Main.Responses;

namespace Main.Interfaces;

public interface IUserBasketService
{
    Task<ApiResponse<UserBasketItemsDTO>> GetUserBasket(Guid id);
    Task<ApiResponse<List<BasketItemResponseDTO>>> ManageUserBucket(Guid userId, AddToBasketRequest request);
}
