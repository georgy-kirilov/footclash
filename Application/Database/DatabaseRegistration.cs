using Microsoft.EntityFrameworkCore;

namespace Application.Database;

public static class DatabaseRegistration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = WriteDbContext.ConnectionString.Get(configuration);
        services.AddDbContext<WriteDbContext>(o => o.UseNpgsql(connection.Value));
        services.AddScoped<ReadDbContext>();
        return services;
    }
}
