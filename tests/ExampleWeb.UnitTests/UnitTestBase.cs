using System;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Microsoft.EntityFrameworkCore;

namespace ExampleWeb.UnitTests;

public class UnitTestBase
{
    private readonly IDatabaseInitializer? _databaseInitializer;
    private readonly DbContextOptions<ExampleDbContext>? _dbContextOptions;

    public UnitTestBase(DatabaseType? databaseType)
    {
        _databaseInitializer = CreateDatabaseInitializer(databaseType);

        _dbContextOptions = _databaseInitializer
            ?.CreateDatabaseGetDbContextOptionsBuilderSync<ExampleDbContext>()
            ?.Options;
    }

    private IDatabaseInitializer? CreateDatabaseInitializer(DatabaseType? databaseType)
    {
        // this is needed if you run tests NOT inside the container
        NpgsqlDatabaseInitializer.ConnectionStringOverride = new ConnectionStringOverride()
        {
            Host = "localhost",
            Port = 5434,
        };
        return databaseType switch
        {
            null => null,
            DatabaseType.Postgres => new NpgsqlDatabaseInitializer(),
            DatabaseType.Sqlite => new SqliteDatabaseInitializer(),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null)
        };
    }

    public ExampleDbContext CreateDbContext()
    {
        if (_dbContextOptions == null)
            return null!;
        return new ExampleDbContext(_dbContextOptions);
    }
}
