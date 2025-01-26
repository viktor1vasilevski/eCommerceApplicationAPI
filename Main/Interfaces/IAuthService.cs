using Main.DTOs.Auth;
using Main.Requests.Auth;
using Main.Responses;

namespace Main.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<RegisterDTO>> RegisterUserAsync(UserRegisterRequest request);
}
