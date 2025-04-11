using Azure.Core;
using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using Main.Constants;
using Main.DTOs.UserBasket;
using Main.Enums;
using Main.Interfaces;
using Main.Requests.UserBasket;
using Main.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Services;

public class UserBasketService(IUnitOfWork<AppDbContext> _uow, ILogger<CategoryService> _logger) : IUserBasketService
{
    private readonly IGenericRepository<UserBasket> _userBasketRepository = _uow.GetGenericRepository<UserBasket>();
    private readonly IGenericRepository<User> _userRepository = _uow.GetGenericRepository<User>();

    public async Task<ApiResponse<List<BasketItemResponseDTO>>> GetBasketItemsByUserId(Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
                return new ApiResponse<List<BasketItemResponseDTO>>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = SharedConstants.INVALID_GUID
                };

            if(!_userRepository.Exists(x => x.Id == userId))
                return new ApiResponse<List<BasketItemResponseDTO>>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = UserBasketConstants.USER_NOT_EXIST,
                };

            var userBasketItems = await _userBasketRepository.GetAsync(x => x.UserId == userId, null, x => x.Include(x => x.Product));

            return new ApiResponse<List<BasketItemResponseDTO>>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Data = userBasketItems.Select(x => new BasketItemResponseDTO
                {
                    Id = x.ProductId,
                    Brand = x.Product.Brand,
                    Name = x.Product.Name,
                    Edition = x.Product.Edition,
                    Quantity = x.Quantity,
                    UnitPrice = (decimal)x.Product.UnitPrice,
                    ImageBase64 = $"data:image/{x.Product.ImageType};base64,{Convert.ToBase64String(x.Product.Image)}"
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in {FunctionName} while getting basket items for user id at {Timestamp}. UserId: {UserId}",
                nameof(GetBasketItemsByUserId), DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), userId);

            return new ApiResponse<List<BasketItemResponseDTO>>
            {
                Success = true,
                NotificationType = NotificationType.ServerError,
                Message = UserBasketConstants.ERROR_GET_USER_BASKET_ITEMS,
            };
        }
    }

    public async Task<ApiResponse<List<BasketItemResponseDTO>>> UpdateBasketForUser(Guid userId, AddToBasketRequest request)
    {
        try
        {
            var user = _userRepository.GetByID(userId);
            if (user is null)
                return new ApiResponse<List<BasketItemResponseDTO>>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = UserBasketConstants.USER_NOT_EXIST
                };

            // Retrieve existing user basket from the database
            var existingItems = _userBasketRepository.Get(x => x.UserId == userId, null, null).ToList();

            foreach (var newItem in request.Items)
            {
                // Check if the product already exists in the basket
                var existingItem = existingItems.FirstOrDefault(x => x.ProductId == newItem.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += newItem.Quantity;
                    await _userBasketRepository.UpdateAsync(existingItem);
                }
                else
                {
                    // If the item does not exist, add it
                    var newBasketItem = new UserBasket
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ProductId = newItem.ProductId,
                        Quantity = newItem.Quantity
                    };

                    await _userBasketRepository.InsertAsync(newBasketItem);
                }
            }

            // Save all changes
            await _uow.SaveChangesAsync();

            // Retrieve updated basket
            var updatedBasket = _userBasketRepository
                .Get(x => x.UserId == userId, null, x => x.Include(x => x.Product))
                .Select(x => new BasketItemResponseDTO
                {
                    Id = x.ProductId,
                    Quantity = x.Quantity,
                    Brand = x.Product.Brand,
                    Name = x.Product.Name,
                    Edition = x.Product.Edition,
                    UnitPrice = (decimal)x.Product.UnitPrice,
                    ImageBase64 = $"data:image/{x.Product.ImageType};base64,{Convert.ToBase64String(x.Product.Image)}",
                }).ToList();

            return new ApiResponse<List<BasketItemResponseDTO>>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = UserBasketConstants.SUCCESS_BASKET_UPDATED,
                Data = updatedBasket
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in {FunctionName} at {Timestamp}. UserId: {UserId}", nameof(UpdateBasketForUser),
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), userId);

            return new ApiResponse<List<BasketItemResponseDTO>>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = UserBasketConstants.ERROR_MANAGE_USER_BASKET_ITEMS
            };
        }
    }

    public async Task<ApiResponse<List<BasketItemResponseDTO>>> RemoveBasketItemForUser(Guid userId, Guid productId)
    {
        try
        {
            var userBasketItems = await _userBasketRepository.GetAsync(x => x.UserId == userId && x.ProductId == productId);
            if (!userBasketItems.Any())
                return new ApiResponse<List<BasketItemResponseDTO>>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = UserBasketConstants.PRODUCT_NOT_FOUND
                };

            _userBasketRepository.Delete(userBasketItems.FirstOrDefault());
            await _uow.SaveChangesAsync();

            var userBasket = _userBasketRepository
                .Get(x => x.UserId == userId, null, x => x.Include(x => x.Product))
                .Select(x => new BasketItemResponseDTO
                {
                    Id = x.ProductId,
                    Quantity = x.Quantity,
                    Brand = x.Product.Brand,
                    Name = x.Product.Name,
                    Edition = x.Product.Edition,
                    UnitPrice = (decimal)x.Product.UnitPrice,
                    ImageBase64 = $"data:image/{x.Product.ImageType};base64,{Convert.ToBase64String(x.Product.Image)}",
                }).ToList();

            return new ApiResponse<List<BasketItemResponseDTO>>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = UserBasketConstants.SUCCESS_REMOVING_USER_BASKET_ITEM,
                Data = userBasket
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in {FunctionName} at {Timestamp}. UserId: {UserId}, ProductId: {ProductId}", nameof(RemoveBasketItemForUser),
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), userId, productId);

            return new ApiResponse<List<BasketItemResponseDTO>>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = UserBasketConstants.ERROR_REMOVING_USER_BASKET_ITEM
            };
        }
    }
}
