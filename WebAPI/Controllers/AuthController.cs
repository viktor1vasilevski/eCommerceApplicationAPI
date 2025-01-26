using Main.Interfaces;
using Main.Requests.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(UserRegisterRequest request)
        {
            var response = await _authService.RegisterUserAsync(request);
            return HandleResponse(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserLoginRequest request)
        {
            //var response = await _authService.RegisterUserAsync(request);
            //return HandleResponse(response);

            return Ok();
        }
    }
}
