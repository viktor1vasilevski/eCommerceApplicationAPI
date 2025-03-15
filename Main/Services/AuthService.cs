using Data.Context;
using EntityModels.Enums;
using EntityModels.Interfaces;
using EntityModels.Models;
using Main.Constants;
using Main.DTOs.Auth;
using Main.Enums;
using Main.Helpers;
using Main.Interfaces;
using Main.Requests.Auth;
using Main.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Main.Services;

public class AuthService(
    IUnitOfWork<AppDbContext> _uow, 
    IConfiguration _configuration, 
    ILogger<CategoryService> _logger) : IAuthService
{
    private readonly IGenericRepository<User> _userRepository = _uow.GetGenericRepository<User>();

    public async Task<ApiResponse<RegisterDTO>> RegisterUserAsync(UserRegisterRequest request)
    {
        try
        {
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
                CreatedBy = Role.Admin.ToString(),
                Created = DateTime.Now
            };

            await _userRepository.InsertAsync(user);
            await _uow.SaveChangesAsync();

            return new ApiResponse<RegisterDTO>
            {
                Success = true,
                Data = new RegisterDTO { Username = user.Username },
                NotificationType = NotificationType.Success,
                Message = AuthConstants.USER_REGISTER_SUCCESS
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while registering user at {Timestamp}. FirstName: {FirstName}, LastName: {LastName}, Username: {Username}, Email: {Email}",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), request.FirstName, request.LastName, request.Username, request.Email);

            return new ApiResponse<RegisterDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = AuthConstants.ERROR_REGISTER
            };
        }
    }

    public async Task<ApiResponse<LoginDTO>> UserLoginAsync(UserLoginRequest request)
    {
        try
        {
            var response = await _userRepository.GetAsync(x => x.Username.ToLower() == request.Username.ToLower());
            var user = response?.FirstOrDefault();

            if (user is null)
            {
                return new ApiResponse<LoginDTO>
                {
                    Message = AuthConstants.USER_NOT_FOUND,
                    Success = false,
                    NotificationType = NotificationType.BadRequest
                };
            }

            var isPasswordValid = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey);

            if (!isPasswordValid)
            {
                return new ApiResponse<LoginDTO>
                {
                    Message = AuthConstants.INVALID_PASSWORD,
                    Success = false,
                    NotificationType = NotificationType.BadRequest
                };
            }

            var token = GenerateJwtToken(user);

            return new ApiResponse<LoginDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = AuthConstants.LOGIN_SUCCESS,
                Data = new LoginDTO 
                { 
                    Id = user.Id,
                    Token = token, 
                    Email = user.Email, 
                    Username = user.Username, 
                    Role = user.Role.ToString() 
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loggin user at {Timestamp}. Username: {Username}",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), request.Username);

            return new ApiResponse<LoginDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = AuthConstants.ERROR_LOGIN
            };
        }
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
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(22),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
