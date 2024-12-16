using Microsoft.EntityFrameworkCore;
using Application.Database;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using static Application.Database.WriteDbContext;

namespace IntegrationTests;

public abstract class TransactionalTestBase : IAsyncLifetime
{
    private readonly string _databaseName;
    private readonly ConnectionString _connectionString;
    private readonly string _adminConnectionString;
    
    private DbTransaction _sharedTransaction = null!;
    private NpgsqlConnection _sharedConnection = null!;
    
    protected TransactionalTestBase()
    {
        _databaseName = $"footclash-{GetType().Name}-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";

        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new(ConnectionString.PgDbSection, _databaseName)
        };

        var insideContainer = Environment.GetEnvironmentVariable("INSIDE_CONTAINER") is not null;
        if (!insideContainer)
        {
            inMemorySettings.Add(new(ConnectionString.PgHostSection, "localhost"));
            inMemorySettings.Add(new(ConnectionString.PgPortSection, "5432"));
            inMemorySettings.Add(new(ConnectionString.PgPasswordSection, "Qwerty1@"));
            inMemorySettings.Add(new(ConnectionString.PgUserSection, "admin"));
        }

        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _connectionString = ConnectionString.Get(configuration);
        _adminConnectionString = _connectionString.Value.Replace(_databaseName, "postgres");
    }
    
    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync();
        _sharedConnection = new NpgsqlConnection(_connectionString.Value);
        await _sharedConnection.OpenAsync();
        
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseNpgsql(_connectionString.Value)
            .Options;

        await using var context = new WriteDbContext(options);
        await context.Database.MigrateAsync();
        _sharedTransaction = await _sharedConnection.BeginTransactionAsync();
    }
    
    private async Task CreateDatabaseAsync()
    {
        await using var adminConnection = new NpgsqlConnection(_adminConnectionString);
        await adminConnection.OpenAsync();

        await using var checkCommand = adminConnection.CreateCommand();
        checkCommand.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{_databaseName}'";
        var exists = await checkCommand.ExecuteScalarAsync();
        
        if (exists is not null)
        {
            return;
        }

        await using var createCommand = adminConnection.CreateCommand();
        createCommand.CommandText = $"CREATE DATABASE \"{_databaseName}\"";
        await createCommand.ExecuteNonQueryAsync();
    }
    
    protected WriteDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseNpgsql(_sharedConnection)
            .Options;

        var context = new WriteDbContext(options);
        context.Database.UseTransaction(_sharedTransaction);
        return context;
    }
    
    public async Task DisposeAsync()
    {
        await _sharedTransaction.RollbackAsync();
        await _sharedTransaction.DisposeAsync();
        await _sharedConnection.DisposeAsync();
        
        await using var adminConnection = new NpgsqlConnection(_adminConnectionString);
        await adminConnection.OpenAsync();

        await using var terminateCommand = adminConnection.CreateCommand();
        
        terminateCommand.CommandText = $"""
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = '{_databaseName}'
            AND pid <> pg_backend_pid();
            """;
        
        await terminateCommand.ExecuteNonQueryAsync();

        await using var dropCommand = adminConnection.CreateCommand();
        dropCommand.CommandText = $"DROP DATABASE IF EXISTS \"{_databaseName}\"";
        await dropCommand.ExecuteNonQueryAsync();
    }
}
