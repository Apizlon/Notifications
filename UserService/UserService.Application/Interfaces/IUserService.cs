using Microsoft.Extensions.Configuration;
using UserService.Application.Contracts.User;
using UserService.Application.Models;

namespace UserService.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<User> GetByUsernameAsync(string username);
    Task<User> GetByIdAsync(string id);
    string GenerateJwtToken(User user, IConfiguration config);
}