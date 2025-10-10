using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UserService.DataAccess.DbContext;

namespace UserService.DataAccess.Extensions;

public static class MigrationsExtensions
{
    public static WebApplication RunMigrations(this WebApplication app)
    {
        Log.Debug("Checking/runngin migrations");
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        dbContext.Database.Migrate();
        Log.Debug("Migrations completed. Database schema is up to date");

        return app;
    }
}