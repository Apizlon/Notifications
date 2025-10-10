namespace UserService.Application.Contracts.User;

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}