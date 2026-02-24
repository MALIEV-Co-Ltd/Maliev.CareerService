# syntax=docker/dockerfile:1.4
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and nuget.config (exclude Tests - filtered by .dockerignore)
COPY nuget.config ./
COPY ["Maliev.CareerService.Api/Maliev.CareerService.Api.csproj", "Maliev.CareerService.Api/"]
COPY ["Maliev.CareerService.Data/Maliev.CareerService.Data.csproj", "Maliev.CareerService.Data/"]

# Restore dependencies with private NuGet feed credentials
RUN --mount=type=secret,id=nuget_username \
    --mount=type=secret,id=nuget_password \
    NUGET_USERNAME=$(cat /run/secrets/nuget_username) \
    NUGET_PASSWORD=$(cat /run/secrets/nuget_password) \
    dotnet restore "Maliev.CareerService.Api/Maliev.CareerService.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/Maliev.CareerService.Api"
RUN dotnet build "Maliev.CareerService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Maliev.CareerService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Ensure 'app' owns the workdir (app user already exists in ASP.NET runtime image)
RUN chown -R app:app /app

# Switch to non-root user
USER app

# Copy published application (now owned by app)
COPY --chown=app:app --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/career/liveness || exit 1

ENTRYPOINT ["dotnet", "Maliev.CareerService.Api.dll"]
