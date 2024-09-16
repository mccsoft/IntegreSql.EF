using System;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Xunit;

namespace ExampleWeb.UnitTests;

public class SeedingFunctionThrowsTests : UnitTestBase
{
    private readonly UserService _service;

    public SeedingFunctionThrowsTests()
        : base(DatabaseType.Sqlite)
    {
        _service = new UserService(CreateDbContext());
    }

    [Fact]
    public void Test()
    {
        string hash = nameof(SeedingFunctionThrowsTests) + Guid.NewGuid();
        SqliteDatabaseInitializer.ClearInitializationTasks();

        Assert.ThrowsAny<Exception>(() =>
        {
            _databaseInitializer?.CreateDatabaseGetDbContextOptionsBuilderSync(
                new DatabaseSeedingOptions<ExampleDbContext>(
                    Name: hash,
                    async context =>
                    {
                        throw new InvalidOperationException("zxc");
                    }
                )
            );
        });
        SqliteDatabaseInitializer.ClearInitializationTasks();

        Assert.ThrowsAny<Exception>(() =>
        {
            _databaseInitializer?.CreateDatabaseGetDbContextOptionsBuilderSync(
                new DatabaseSeedingOptions<ExampleDbContext>(
                    Name: hash,
                    async context =>
                    {
                        throw new InvalidOperationException("zxc");
                    }
                )
            );
        });
    }
}
