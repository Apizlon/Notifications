using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Models;

namespace NotificationService.DataAccess.DbContext;

public class NotificationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Notification> Notifications { get; set; }

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }
}