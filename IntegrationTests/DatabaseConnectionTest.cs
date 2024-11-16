using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public sealed class DatabaseConnectionTest : TransactionalTestBase
{
    [Fact]
    public async Task PingDatabase_ShouldReturnCorrectResult()
    {
        // Arrange
        await using var context = CreateDbContext();

        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT 1";
        await context.Database.OpenConnectionAsync();

        // Act
        var result = (int?)await command.ExecuteScalarAsync();

        // Assert
        Assert.Equal(1, result);
    }
}
