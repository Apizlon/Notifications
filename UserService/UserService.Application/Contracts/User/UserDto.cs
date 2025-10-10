namespace UserService.Application.Contracts.User;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
}