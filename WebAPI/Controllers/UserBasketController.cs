using Main.Interfaces;
using Main.Requests.UserBasket;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserBasketController(IUserBasketService userBasketService) : BaseController
    {
        private readonly IUserBasketService _userBasketService = userBasketService;

        [HttpPost("ManageBasketByUserId/{userId}")]
        public async Task<IActionResult> ManageBasketByUserId([FromRoute] Guid userId, [FromBody] AddToBasketRequest request)
        {
            var response = await _userBasketService.ManageUserBucket(userId, request);
            return HandleResponse(response);
        }
    }
}
