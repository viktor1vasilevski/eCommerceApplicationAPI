using Main.DTOs.Auth;
using Main.Interfaces;
using Main.Requests.Auth;
using Main.Responses;

namespace Main.Services;

public class AuthService : IAuthService
{
    public Task<ApiResponse<RegisterDTO>> RegisterUserAsync(UserRegisterRequest request)
    {
        throw new NotImplementedException();
    }
}
