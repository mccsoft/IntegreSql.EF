using ExampleWebPostgresSpecific.Database;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Microsoft.EntityFrameworkCore;

namespace ExampleWebPostgresSpecific.UnitTests;

public class UnitTestBase : IDisposable
{
    protected readonly IDatabaseInitializer? _databaseInitializer;
    private DbContextOptions<ExamplePostgresSpecificDbContext>? _dbContextOptions;

    public UnitTestBase(
        DatabaseType? databaseType,
        DatabaseSeedingOptions<ExamplePostgresSpecificDbContext>? seedingOptions = null
    )
    {
        _databaseInitializer = CreateDatabaseInitializer(databaseType);

        _dbContextOptions = _databaseInitializer
            ?.CreateDatabaseGetDbContextOptionsBuilderSync<ExamplePostgresSpecificDbContext>(
                seedingOptions: seedingOptions
            )
            ?.Options;
    }

    private IDatabaseInitializer? CreateDatabaseInitializer(DatabaseType? databaseType)
    {
        return databaseType switch
        {
            null => null,
            DatabaseType.Postgres
                => new NpgsqlDatabaseInitializer(
                    // This is needed if you run tests NOT inside the container.
                    // 5434 is the public port number of Postgresql instance
                    connectionStringOverride: new() { Host = "localhost", Port = 5434 },
                    adjustNpgsqlDataSource: builder => builder.EnableDynamicJson()
                )
                {
                    DropDatabaseOnRemove = true,
                },
            DatabaseType.Sqlite => new SqliteDatabaseInitializer(),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null)
        };
    }

    public ExamplePostgresSpecificDbContext CreateDbContext()
    {
        if (_dbContextOptions == null)
            return null!;
        return new ExamplePostgresSpecificDbContext(_dbContextOptions);
    }

    public void Dispose()
    {
        _databaseInitializer?.RemoveDatabase(CreateDbContext().Database.GetConnectionString());
    }
}
