using Main.Interfaces;
using Main.Requests.UserBasket;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserBasketController(IUserBasketService userBasketService) : BaseController
    {
        private readonly IUserBasketService _userBasketService = userBasketService;

        [HttpPost("UpdateBasketForUser/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> UpdateBasketForUser([FromRoute] Guid userId, [FromBody] AddToBasketRequest request)
        {
            var response = await _userBasketService.UpdateBasketForUser(userId, request);
            return HandleResponse(response);
        }


        [HttpGet("GetBasketItemsByUserId/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetBasketByUserId([FromRoute] Guid userId)
        {
            var response = await _userBasketService.GetBasketItemsByUserId(userId);
            return HandleResponse(response);
        }

        [HttpDelete("RemoveBasketItemsForUser/{userId}/{itemId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> Delete([FromRoute] Guid userId, [FromRoute] Guid itemId)
        {
            var response = await _userBasketService.RemoveBasketItemForUser(userId, itemId);
            return HandleResponse(response);
        }

        [HttpDelete("RemoveAllBasketItemsForUser/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> DeleteAll([FromRoute] Guid userId)
        {
            var response = await _userBasketService.RemoveAllBasketItemsForUser(userId);
            return HandleResponse(response);
        }

    }
}
