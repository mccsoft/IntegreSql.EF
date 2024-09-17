using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MccSoft.IntegreSql.EF.DatabaseInitialization;

public abstract class BaseDatabaseInitializer : IDatabaseInitializer
{
    /// <summary>
    /// Creates a template database (if not created before) and seeds the data by
    /// running a <paramref name="initializeDatabase"/> function (which receives connection string).
    /// Then it creates a copy of template database to be used in each test
    /// and returns a connection string to be used in the test.
    /// Normally you should run this function once per test.
    /// </summary>
    /// <param name="databaseHash">Hash that uniquely identifies your database structure + seed data</param>
    /// <param name="initializeDatabase">
    /// Function that should create DB schema and seed the data.
    /// Receives a connection string.
    /// </param>
    /// <returns>Connection string to a copy of template database</returns>
    protected abstract Task<string> CreateDatabaseGetConnectionStringInternal(
        string databaseHash,
        Func<string, Task> initializeDatabase
    );

    /// <inheritdoc cref="IDatabaseInitializer.ReturnDatabaseToPool"/>
    public abstract Task ReturnDatabaseToPool(string connectionString);

    /// <inheritdoc cref="IDatabaseInitializer.ReturnDatabaseToPool"/>
    public virtual void ReturnDatabaseToPoolSync(string connectionString)
    {
        TaskUtils.RunSynchronously(() => ReturnDatabaseToPool(connectionString));
    }

    /// <inheritdoc cref="IDatabaseInitializer.RemoveDatabase"/>
    public abstract Task RemoveDatabase(string connectionString);

    /// <inheritdoc cref="IDatabaseInitializer.RemoveDatabaseSync"/>
    public virtual void RemoveDatabaseSync(string connectionString)
    {
        TaskUtils.RunSynchronously(() => RemoveDatabase(connectionString));
    }

    /// <inheritdoc cref="IDatabaseInitializer.UseProvider"/>
    public abstract void UseProvider(DbContextOptionsBuilder options, string connectionString);

    /// <inheritdoc cref="IDatabaseInitializer.UseProvider{TDbContext}"/>
    public void UseProvider<TDbContext>(
        DbContextOptionsBuilder options,
        DatabaseSeedingOptions<TDbContext> databaseSeedingOptions
    )
        where TDbContext : DbContext
    {
        string connectionString = CreateDatabaseGetConnectionString(databaseSeedingOptions)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        options.ConfigureWarnings(x => x.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
        UseProvider(options, connectionString);
    }

    protected virtual string AdjustConnectionStringOnSeeding(string connectionString)
    {
        return connectionString;
    }

    /// <inheritdoc cref="IDatabaseInitializer.CreateDatabaseGetConnectionString{TDbContext}"/>
    public Task<string> CreateDatabaseGetConnectionString<TDbContext>(
        DatabaseSeedingOptions<TDbContext> databaseSeeding
    )
        where TDbContext : DbContext
    {
        string lastMigrationName = ContextHelper.GetLastMigrationName<TDbContext>() ?? "";

        return CreateDatabaseGetConnectionStringInternal(
            databaseSeeding?.Name
                + nameof(CreateDatabaseGetConnectionString)
                + lastMigrationName
                + typeof(TDbContext).Assembly.FullName,
            async (connectionString) =>
            {
                await using var dbContext = ContextHelper.CreateDbContext<TDbContext>(
                    useProvider: this,
                    connectionString: AdjustConnectionStringOnSeeding(connectionString),
                    factoryMethod: databaseSeeding?.DbContextFactory
                );
                if (databaseSeeding?.DisableEnsureCreated != true)
                {
                    dbContext.Database.EnsureCreated();
                }

                PerformBasicSeedingOperations(dbContext);

                await (databaseSeeding?.SeedingFunction?.Invoke(dbContext) ?? Task.CompletedTask);
            }
        );
    }

    /// <inheritdoc cref="IDatabaseInitializer.CreateDatabaseGetConnectionStringSync{TDbContext}"/>
    public string CreateDatabaseGetConnectionStringSync<TDbContext>(
        DatabaseSeedingOptions<TDbContext> databaseSeeding = null
    )
        where TDbContext : DbContext
    {
        return TaskUtils.RunSynchronously(() => CreateDatabaseGetConnectionString(databaseSeeding));
    }

    /// <inheritdoc cref="IDatabaseInitializer.CreateDbContextOptionsBuilder{TDbContext}"/>
    public virtual DbContextOptionsBuilder<TDbContext> CreateDbContextOptionsBuilder<TDbContext>(
        string connectionString
    )
        where TDbContext : DbContext
    {
        var builder = new DbContextOptionsBuilder<TDbContext>();
        UseProvider(builder, connectionString);

        builder.EnableSensitiveDataLogging().EnableDetailedErrors();
        return builder;
    }

    /// <inheritdoc cref="IDatabaseInitializer.CreateDatabaseGetDbContextOptionsBuilder{TDbContext}"/>
    public virtual async Task<
        DbContextOptionsBuilder<TDbContext>
    > CreateDatabaseGetDbContextOptionsBuilder<TDbContext>(
        DatabaseSeedingOptions<TDbContext> seedingOptions
    )
        where TDbContext : DbContext
    {
        var connectionString = await CreateDatabaseGetConnectionString(seedingOptions);
        return CreateDbContextOptionsBuilder<TDbContext>(connectionString);
    }

    /// <inheritdoc cref="IDatabaseInitializer.CreateDatabaseGetDbContextOptionsBuilderSync{TDbContext}"/>
    public virtual DbContextOptionsBuilder<TDbContext> CreateDatabaseGetDbContextOptionsBuilderSync<TDbContext>(
        DatabaseSeedingOptions<TDbContext> seedingOptions = null
    )
        where TDbContext : DbContext
    {
        return TaskUtils.RunSynchronously(
            () => CreateDatabaseGetDbContextOptionsBuilder(seedingOptions)
        );
    }

    protected abstract void PerformBasicSeedingOperations(DbContext dbContext);

    public virtual void Dispose() { }
}
