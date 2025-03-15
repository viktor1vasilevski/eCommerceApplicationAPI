﻿using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
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

    public Task<ApiResponse<UserBasketItemsDTO>> GetUserBasket(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<List<BasketItemResponseDTO>>> ManageUserBucket(Guid userId, AddToBasketRequest request)
    {
        try
        {
            var userExist = _userRepository.GetByID(userId);
            if (userExist is null)
                return new ApiResponse<List<BasketItemResponseDTO>>
                {
                    Success = false
                };


            // Retrieve existing user basket from the database
            var existingItems = _userBasketRepository.Get(x => x.UserId == userId, null, null).ToList();

            foreach (var newItem in request.Items)
            {
                // Check if the product already exists in the basket
                var existingItem = existingItems.FirstOrDefault(x => x.ProductId == newItem.ProductId);

                if (existingItem != null)
                {
                    // If the item exists, update its quantity
                    existingItem.Quantity += newItem.Quantity;
                    _userBasketRepository.Update(existingItem);
                }
                else
                {
                    // If the item does not exist, add it
                    var newBasketItem = new UserBasket
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ProductId = newItem.ProductId,
                        Quantity = newItem.Quantity,
                    };

                    await _userBasketRepository.InsertAsync(newBasketItem);
                }
            }

            // Save all changes
            _uow.SaveChanges();

            // Retrieve updated basket
            var updatedBasket = _userBasketRepository
                .Get(x => x.UserId == userId, null, x => x.Include(x => x.Product))
                .Select(x => new BasketItemResponseDTO
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    ProductBrand = x.Product.Name,
                    ProductEdition = x.Product.Edition,
                    Price = (decimal)x.Product.UnitPrice,
                    ImageBase64 = $"data:image/{x.Product.ImageType};base64,{Convert.ToBase64String(x.Product.Image)}",
                }).ToList();

            //var response = new UserBasketItemsDTO {  Items = updatedBasket };

            return new ApiResponse<List<BasketItemResponseDTO>>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Data = updatedBasket
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<BasketItemResponseDTO>>
            {
                Success = false,
            };
        }
    }

}
