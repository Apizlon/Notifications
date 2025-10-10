namespace UserService.Application.Contracts.User;

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string? Email { get; set; }
    public string Password { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
}