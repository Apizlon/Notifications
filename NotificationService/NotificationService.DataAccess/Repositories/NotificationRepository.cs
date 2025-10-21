using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using NotificationService.DataAccess.DbContext;

namespace NotificationService.DataAccess.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task AddBatchAsync(List<Notification> notifications)
    {
        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetLastThreeByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(3)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetPaginatedByUserIdAsync(Guid userId, int page, int pageSize)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            return false;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }
}