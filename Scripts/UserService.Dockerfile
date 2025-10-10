FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

ARG BUILD_CONFIG=Release

# Copy the solution and restore dependencies
COPY UserService/UserService.Api/UserService.Api.csproj ./UserService.Api/
COPY UserService/UserService.Application/UserService.Application.csproj ./UserService.Application/
COPY UserService/UserService.DataAccess/UserService.DataAccess.csproj ./UserService.DataAccess/
COPY UserService/UserService.sln .
RUN dotnet restore ./UserService.sln

# Copy the rest of the code and build
COPY UserService/UserService.Api/ ./UserService.Api/
COPY UserService/UserService.Application/ ./UserService.Application/
COPY UserService/UserService.DataAccess/ ./UserService.DataAccess/
WORKDIR /app/UserService.Api
RUN dotnet publish -c $BUILD_CONFIG -o out

# Use the runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/UserService.Api/out ./
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
