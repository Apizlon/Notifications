using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Contracts.User;
using UserService.Application.Exceptions;
using UserService.Application.Interfaces;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Registering new user with username {Username}", request.Username);
        var user = await _userService.RegisterAsync(request);
        _logger.LogInformation("User registered successfully: {UserId}", user.Id);
        return user;
    }

    [HttpPost("login")]
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Logging in user with username {Username}", request.Username);
        var response = await _userService.LoginAsync(request);
        _logger.LogInformation("User logged in successfully: {UserId}", response.User.Id);
        return response;
    }

    [HttpPut("update")]
    [Authorize]
    public async Task<UserDto> UpdateUserAsync(UpdateUserRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new BadRequestException("Invalid token");
        _logger.LogInformation("Updating user with id {UserId}", userId);
        var user = await _userService.UpdateUserAsync(userId, request);
        _logger.LogInformation("User updated successfully: {UserId}", userId);
        return user;
    }
}