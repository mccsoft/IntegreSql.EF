using ExampleWebPostgresSpecific.Database;
using MccSoft.IntegreSql.EF;
using Microsoft.EntityFrameworkCore;

namespace ExampleWebPostgresSpecific.UnitTests;

public class UnitTestSimplified : IDisposable
{
    private readonly DbContextOptions<ExamplePostgresSpecificDbContext> _dbContextOptions;
    private readonly NpgsqlDatabaseInitializer _databaseInitializer;

    public UnitTestSimplified()
    {
        _databaseInitializer = new NpgsqlDatabaseInitializer(
            // This is needed if you run tests NOT inside the container.
            // 5434 is the public port number of Postgresql instance
            connectionStringOverride: new() { Host = "localhost", Port = 5434, }
        )
        {
            DropDatabaseOnRemove = true,
        };
        _dbContextOptions = _databaseInitializer
            .CreateDatabaseGetDbContextOptionsBuilderSync<ExamplePostgresSpecificDbContext>()
            .Options;
    }

    public ExamplePostgresSpecificDbContext CreateDbContext()
    {
        return new ExamplePostgresSpecificDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task Test1()
    {
        var connectionString = CreateDbContext().Database.GetConnectionString();
        var service = new UserService(CreateDbContext());
        var users = await service.GetUsers();
        Assert.Equal(new[] { "John", "Bill" }, users);
    }

    public void Dispose()
    {
        _databaseInitializer?.RemoveDatabase(CreateDbContext().Database.GetConnectionString());
    }
}
