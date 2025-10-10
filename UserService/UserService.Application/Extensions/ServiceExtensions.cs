using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces;

namespace UserService.Application.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, Services.UserService>();
        return services;
    }
}