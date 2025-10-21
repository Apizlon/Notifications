# NotificationService.Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

ARG BUILD_CONFIG=Release

# Copy the solution and restore dependencies
COPY NotificationService/NotificationService.Api/NotificationService.Api.csproj ./NotificationService.Api/
COPY NotificationService/NotificationService.Application/NotificationService.Application.csproj ./NotificationService.Application/
COPY NotificationService/NotificationService.DataAccess/NotificationService.DataAccess.csproj ./NotificationService.DataAccess/
COPY NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj ./NotificationService.Infrastructure/
COPY NotificationService/NotificationService.sln .
RUN dotnet restore ./NotificationService.sln

# Copy the rest of the code and build
COPY NotificationService/NotificationService.Api/ ./NotificationService.Api/
COPY NotificationService/NotificationService.Application/ ./NotificationService.Application/
COPY NotificationService/NotificationService.DataAccess/ ./NotificationService.DataAccess/
COPY NotificationService/NotificationService.Infrastructure/ ./NotificationService.Infrastructure/
WORKDIR /app/NotificationService.Api
RUN dotnet publish -c $BUILD_CONFIG -o out

# Use the runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/NotificationService.Api/out ./
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
