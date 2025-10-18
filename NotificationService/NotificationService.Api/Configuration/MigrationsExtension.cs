using Microsoft.EntityFrameworkCore;
using NotificationService.DataAccess.DbContext;
using Serilog;

namespace NotificationService.Api.Configuration;

public static class MigrationsExtensions
{
    public static WebApplication RunMigrations(this WebApplication app)
    {
        Log.Debug("Checking/runngin migrations");
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        dbContext.Database.Migrate();
        Log.Debug("Migrations completed. Database schema is up to date");

        return app;
    }
}