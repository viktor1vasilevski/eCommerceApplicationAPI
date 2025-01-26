using Data.Context;
using EntityModels.Enums;
using EntityModels.Interfaces;
using EntityModels.Models;
using FluentValidation;
using Main.Constants;
using Main.DTOs.Auth;
using Main.Enums;
using Main.Helpers;
using Main.Interfaces;
using Main.Requests.Auth;
using Main.Responses;
using Main.Validations.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Main.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork<AppDbContext> _uow;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IConfiguration _configuration;

    private readonly IValidator<UserRegisterRequest> _userRegisterRequestValidator;
    public AuthService(IUnitOfWork<AppDbContext> uow, IConfiguration configuration, IValidator<UserRegisterRequest> userRegisterRequestValidator)
    {
        _uow = uow;
        _userRepository = _uow.GetGenericRepository<User>();
        _configuration = configuration;

        _userRegisterRequestValidator = userRegisterRequestValidator;
    }


    public async Task<ApiResponse<RegisterDTO>> RegisterUserAsync(UserRegisterRequest request)
    {
        try
        {
            var validationResult = ValidationHelper.ValidateRequest<UserRegisterRequest, RegisterDTO>(request, _userRegisterRequestValidator);

            if (validationResult != null)
                return validationResult;

            var userExist = await _userRepository.ExistsAsync(x => x.Email.ToLower() == request.Email.ToLower() || x.Username.ToLower() == request.Username.ToLower());
            if (userExist)
                return new ApiResponse<RegisterDTO> { Success = false, NotificationType = NotificationType.BadRequest, Message = AuthConstants.USER_EXISTS };

            var saltKey = GenerateSalt();
            var hash = PasswordHasher.HashPassword(request.Password, saltKey);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Username = request.Username,
                PasswordHash = hash,
                SaltKey = Convert.ToBase64String(saltKey),
                Role = Role.Customer,
                CreatedBy = "Admin",
                Created = DateTime.Now
            };

            await _userRepository.InsertAsync(user);
            await _uow.SaveChangesAsync();

            return new ApiResponse<RegisterDTO>
            {
                Success = true,
                Data = new RegisterDTO { Id = user.Id, Username = user.Username },
                NotificationType = NotificationType.Success,
                Message = AuthConstants.USER_REGISTER_SUCCESS
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<RegisterDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = AuthConstants.ERROR_REGISTER
            };
        }
    }

    public Task<ApiResponse<LoginDTO>> UserLoginAsync(UserLoginRequest request)
    {
        throw new NotImplementedException();
    }

    private static byte[] GenerateSalt(int size = 16)
    {
        byte[] salt = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        return salt;
    }
    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["JwtSettings:Secret"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
