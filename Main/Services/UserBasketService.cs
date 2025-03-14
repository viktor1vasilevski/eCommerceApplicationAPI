using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using Main.DTOs.UserBasket;
using Main.Interfaces;
using Main.Responses;
using Microsoft.Extensions.Logging;

namespace Main.Services;

public class UserBasketService(IUnitOfWork<AppDbContext> _uow, ILogger<CategoryService> _logger) : IUserBasketService
{
    private readonly IGenericRepository<UserBasket> _userBasketRepository = _uow.GetGenericRepository<UserBasket>();
    public async Task<ApiResponse<UserBasketItemsDTO>> GetUserBasket()
    {
        throw new NotImplementedException();
    }
}
