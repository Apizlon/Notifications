using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Models;

namespace NotificationService.DataAccess.DbContext;

public class NotificationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<Notification> Notifications { get; set; }

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });
    }
}