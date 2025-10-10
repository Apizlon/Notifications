using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UserService.Application.Interfaces;
using UserService.Application.Models;
using UserService.DataAccess.DbContext;

namespace UserService.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> ExistsByEmailAsync(string? email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task CreateAsync(User user, IDbContextTransaction? transaction = null)
    {
        var tx = transaction ?? await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            if (transaction == null) await tx.CommitAsync();
        }
        catch
        {
            if (transaction == null) await tx.RollbackAsync();
            throw;
        }
        finally
        {
            if (transaction == null) await tx.DisposeAsync();
        }
    }

    public async Task UpdateAsync(User user, IDbContextTransaction? transaction = null)
    {
        var tx = transaction ?? await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            if (transaction == null) await tx.CommitAsync();
        }
        catch
        {
            if (transaction == null) await tx.RollbackAsync();
            throw;
        }
        finally
        {
            if (transaction == null) await tx.DisposeAsync();
        }
    }
}