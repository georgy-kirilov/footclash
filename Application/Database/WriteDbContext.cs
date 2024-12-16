using Microsoft.EntityFrameworkCore;

namespace Application.Database;

public sealed class WriteDbContext(DbContextOptions<WriteDbContext> options) : DbContext(options)
{
    public sealed record ConnectionString
    {
        public const string PgDbSection = "PG_DB";
        public const string PgHostSection = "PG_HOST";
        public const string PgPortSection = "PG_PORT";
        public const string PgPasswordSection = "PG_PASSWORD";
        public const string PgUserSection = "PG_USER";
        
        private ConnectionString(string value) => Value = value;

        public string Value { get; }

        public static ConnectionString Get(IConfiguration configuration)
        {
            var host = configuration.GetRequiredSection("PG_HOST").Get<string>();
            var port = configuration.GetRequiredSection("PG_PORT").Get<string>();
            var database = configuration.GetRequiredSection(PgDbSection).Get<string>();
            var user = configuration.GetRequiredSection("PG_USER").Get<string>();
            var password = configuration.GetRequiredSection("PG_PASSWORD").Get<string>();
            var connection = $"Host={host};Port={port};Database={database};Username={user};Password={password};";
            return new ConnectionString(connection);
        }
    }
}
