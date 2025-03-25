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

        [HttpPost("ManageBasketItemsByUserId")]
        public async Task<IActionResult> ManageBasketByUserId([FromBody] AddToBasketRequest request)
        {
            var response = await _userBasketService.ManageBasketItemsByUserId(request);
            return HandleResponse(response);
        }


        [HttpGet("GetBasketItemsByUserId/{userId}")]
        public async Task<IActionResult> GetBasketByUserId([FromRoute] Guid userId)
        {
            var response = await _userBasketService.GetBasketItemsByUserId(userId);
            return HandleResponse(response);
        }

        [HttpDelete("RemoveBasketItemsForUser/{userId}/{itemId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public IActionResult Delete([FromRoute] Guid userId, Guid itemId)
        {
            var response = _userBasketService.RemoveBasketItemForUser(userId, itemId);
            //return HandleResponse(response);
            return Ok();
        }

    }
}
