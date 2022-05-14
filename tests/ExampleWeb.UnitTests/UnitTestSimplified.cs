using System;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExampleWeb.UnitTests;

public class UnitTestSimplified : IDisposable
{
    private readonly DbContextOptions<ExampleDbContext> _dbContextOptions;
    private readonly NpgsqlDatabaseInitializer _databaseInitializer;

    public UnitTestSimplified()
    {
        _databaseInitializer = new NpgsqlDatabaseInitializer(
            // This is needed if you run tests NOT inside the container.
            // 5434 is the public port number of Postgresql instance
            connectionStringOverride: new() { Host = "localhost", Port = 5434, }
        );
        _dbContextOptions = _databaseInitializer
            .CreateDatabaseGetDbContextOptionsBuilderSync<ExampleDbContext>()
            .Options;
    }

    public ExampleDbContext CreateDbContext()
    {
        return new ExampleDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task Test1()
    {
        var service = new UserService(CreateDbContext());
        var users = await service.GetUsers();
        Assert.Equal(new[] { "John", "Bill" }, users);
    }

    public void Dispose()
    {
        _databaseInitializer?.RemoveDatabase(CreateDbContext().Database.GetConnectionString());
    }
}
