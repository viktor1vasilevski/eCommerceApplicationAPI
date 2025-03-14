using Main.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserBasketController(IUserBasketService userBasketService) : BaseController
    {
        private readonly IUserBasketService _userBasketService = userBasketService;


    }
}
