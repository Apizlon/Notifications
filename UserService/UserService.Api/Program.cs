using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using UserService.Api.Configuration;
using UserService.Api.Filters;
using UserService.Application.Extensions;
using UserService.DataAccess.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureSerilog();

builder.Services.AddControllers(options => options.Filters.Add<CustomExceptionFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Secret"] ?? throw new InvalidOperationException("JWT Secret is required"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"] ?? "UserService",
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"] ?? "AllServices",
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddServices();
builder.Services.AddRepositories(builder.Configuration);

var app = builder.Build();

app.RunMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();