using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotificationService.Application.Interfaces;
using Serilog;

namespace NotificationService.Api.Configuration;

public static class ConfigurationExtension
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                );
        });
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<INotificationSender, Services.SignalRNotificationSender>();
        return services;
    }

    public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;

                // Простая настройка для SignalR: извлечение токена из query (фикс 401 на negotiate)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var pathString = context.HttpContext.Request.Path;
                        var accessToken = context.Request.Query["access_token"].FirstOrDefault();

                        // Если запрос к SignalR-хабу, устанавливаем токен из query
                        if (!string.IsNullOrEmpty(accessToken) && pathString.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        // Для API fallback на header (автоматически, если нет query)

                        return Task.CompletedTask;
                    }
                };

                // Базовая валидация Symmetric JWT (как в UserService)
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!))
                };
            });
        return services;
    }
}