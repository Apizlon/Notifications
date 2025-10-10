namespace UserService.Application.Models;

public class User
{
    public string Id { get; set; }  = null!;
    public string Username { get; set; } = null!;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
}