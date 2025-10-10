namespace UserService.Application.Contracts.User;

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
}