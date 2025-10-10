using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Contracts.User;
using UserService.Application.Exceptions;
using UserService.Application.Extensions;
using UserService.Application.Interfaces;
using UserService.Application.Models;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IConfiguration _config;

    public UserService(IUserRepository repository, IConfiguration config)
    {
        _repository = repository;
        _config = config;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        request.ValidateRegister();
        if (await _repository.ExistsByUsernameAsync(request.Username))
            throw new BadRequestException("Username already exists");

        if (!string.IsNullOrEmpty(request.Email) && await _repository.ExistsByEmailAsync(request.Email))
            throw new BadRequestException("Email already exists");

        var user = request.ToDomain();
        await _repository.CreateAsync(user);
        return user.ToDto();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        request.ValidateLogin();
        var user = await _repository.GetByUsernameAsync(request.Username) ??
                   throw new BadRequestException("Invalid username or password");
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Invalid username or password");

        var token = GenerateJwtToken(user, _config);
        return user.ToLoginResponse(token);
    }

    public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        request.ValidateUpdate();
        var user = await _repository.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
        user.ApplyUpdate(request);
        await _repository.UpdateAsync(user);
        return user.ToDto();
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _repository.GetByUsernameAsync(username)?? throw new NotFoundException($"User with username {username} not found");
    }

    public async Task<User> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id) ?? throw new NotFoundException($"User with id {id} not found");
    }

    public string GenerateJwtToken(User user, IConfiguration config)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username)
        };
        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(int.Parse(config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}