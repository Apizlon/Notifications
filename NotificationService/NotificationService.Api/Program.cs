using Microsoft.OpenApi.Models;
using NotificationService.Api.Configuration;
using NotificationService.Api.Filters;
using NotificationService.Api.Hubs;
using NotificationService.Application.Extensions;
using NotificationService.DataAccess.Extensions;
using NotificationService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

builder.Services.AddControllers(options => options.Filters.Add<CustomExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService API", Version = "v1" });

        // JWT Security для Bearer токенов
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    });
}
else
{
    builder.Services.AddSwaggerGen();
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddRepositories(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddApiServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

app.RunMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
app.MapHub<NotificationHub>("/notificationHub").RequireCors("AllowFrontend");;

app.Run();