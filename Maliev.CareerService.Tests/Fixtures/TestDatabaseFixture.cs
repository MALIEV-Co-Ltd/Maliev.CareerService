using Maliev.CareerService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Testcontainers.RabbitMq;
using Xunit;

namespace Maliev.CareerService.Tests.Fixtures;

/// <summary>
/// Test fixture for managing PostgreSQL, Redis, and RabbitMQ test containers using Testcontainers
/// Provides clean infrastructure for each test class
/// </summary>
public class TestDatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;
    private RabbitMqContainer? _rabbitmqContainer;

    public string ConnectionString { get; private set; } = string.Empty;
    public string RedisConnectionString { get; private set; } = string.Empty;
    public string RabbitMqConnectionString { get; private set; } = string.Empty;
    private bool _initialized = false;

    public TestDatabaseFixture()
    {
        // Container will be initialized in InitializeAsync
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        _postgresContainer = 
                #pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18-alpine")
            .WithDatabase("career_service_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder().WithImage("redis:8.4-alpine")
            .Build();

        _rabbitmqContainer = new RabbitMqBuilder().WithImage("rabbitmq:4.2-alpine")
            .Build();
#pragma warning restore CS0618

        // Start all containers in parallel
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitmqContainer.StartAsync()
        );

        ConnectionString = _postgresContainer.GetConnectionString();
        RedisConnectionString = _redisContainer.GetConnectionString();
        RabbitMqConnectionString = _rabbitmqContainer.GetConnectionString();

        // Wait for Redis to be ready
        using (var connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(RedisConnectionString))
        {
            await connection.GetDatabase().PingAsync();
        }

        // Test connection and run migrations
        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString)
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        await using var context = new CareerDbContext(optionsBuilder.Options);

        // Run migrations only (EnsureCreated + Migrate together causes pending model changes warning)
        await context.Database.EnsureCreatedAsync();

        _initialized = true;
    }

    public async Task DisposeAsync()
    {
        // Stop and remove all containers
        if (_postgresContainer != null)
        {
            try
            {
                await _postgresContainer.DisposeAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        if (_redisContainer != null)
        {
            try
            {
                await _redisContainer.DisposeAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        if (_rabbitmqContainer != null)
        {
            try
            {
                await _rabbitmqContainer.DisposeAsync();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Creates a new DbContext instance for testing
    /// </summary>
    public CareerDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString)
            .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        return new CareerDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Cleans all data from the database while preserving schema
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        await using var context = CreateDbContext();

        // Dynamically get all table names from the model
        var tableNames = context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(t => t != null)
            .Cast<string>()
            .ToList();

        // Truncate all tables in reverse order (to respect foreign keys if needed, though CASCADE is used)
        foreach (var tableName in tableNames)
        {
            try
            {
                // Table names are from the model, not user input - safe from SQL injection
#pragma warning disable EF1002, EF1003
                await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{tableName}\" RESTART IDENTITY CASCADE");
#pragma warning restore EF1002, EF1003
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
            {
                // Table doesn't exist - ignore this error
            }
        }
    }
}




