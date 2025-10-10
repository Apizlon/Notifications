using UserService.Application.Contracts.User;
using UserService.Application.Models;

namespace UserService.Application.Extensions;

public static class UserMappingExtensions
{
    public static User ToDomain(this RegisterRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DateOfBirth = request.DateOfBirth,
            CreatedAt = DateTime.UtcNow
        };
        return user;
    }

    public static User ApplyUpdate(this User user, UpdateUserRequest request)
    {
        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;
        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = request.DateOfBirth;
        return user;
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            CreatedAt = user.CreatedAt
        };
    }

    public static LoginResponse ToLoginResponse(this User user, string token)
    {
        return new LoginResponse
        {
            Token = token,
            User = user.ToDto()
        };
    }
}