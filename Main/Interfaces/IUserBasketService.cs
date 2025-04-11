using Main.DTOs.UserBasket;
using Main.Requests.UserBasket;
using Main.Responses;

namespace Main.Interfaces;

public interface IUserBasketService
{
    Task<ApiResponse<List<BasketItemResponseDTO>>> GetBasketItemsByUserId(Guid userId);
    Task<ApiResponse<List<BasketItemResponseDTO>>> UpdateBasketForUser(Guid userId, AddToBasketRequest request);
    Task<ApiResponse<List<BasketItemResponseDTO>>> RemoveBasketItemForUser(Guid userId, Guid productId);
}
