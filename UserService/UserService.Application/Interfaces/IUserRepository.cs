using Microsoft.EntityFrameworkCore.Storage;
using UserService.Application.Models;

namespace UserService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string? email);
    Task CreateAsync(User user, IDbContextTransaction? transaction = null);
    Task UpdateAsync(User user, IDbContextTransaction? transaction = null);
}