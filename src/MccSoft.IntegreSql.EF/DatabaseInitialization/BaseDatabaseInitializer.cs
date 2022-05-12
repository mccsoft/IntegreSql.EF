using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSql.EF.DatabaseInitialization;

public abstract class BaseDatabaseInitializer : IDatabaseInitializer
{
    /// <inheritdoc cref="IDatabaseInitializer.CreateDatabaseGetConnectionStringAdvanced"/>
    public abstract Task<string> CreateDatabaseGetConnectionStringAdvanced(
        string databaseHash,
        Func<string, Task> initializeDatabase
    );

    /// <inheritdoc cref="IDatabaseInitializer.CreateDatabaseGetConnectionStringAdvancedSync"/>
    public string CreateDatabaseGetConnectionStringAdvancedSync(
        string databaseHash,
        Func<string, Task> initializeDatabase
    )
    {
        return TaskUtils.RunSynchronously(
            () => CreateDatabaseGetConnectionStringAdvanced(databaseHash, initializeDatabase)
        );
    }

    /// <inheritdoc cref="IDatabaseInitializer.ReturnDatabaseToPool"/>
    public abstract Task ReturnDatabaseToPool(string connectionString);

    /// <inheritdoc cref="IDatabaseInitializer.UseProvider"/>
    public abstract void UseProvider(DbContextOptionsBuilder options, string connectionString);

    /// <inheritdoc cref="IDatabaseInitializer.UseProvider{TDbContext}"/>
    public void UseProvider<TDbContext>(
        DbContextOptionsBuilder options,
        BasicDatabaseSeedingOptions<TDbContext> databaseSeedingOptions
    ) where TDbContext : DbContext
    {
        string connectionString = CreateDatabaseGetConnectionString(databaseSeedingOptions)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        UseProvider(options, connectionString);
    }

    /// <inheritdoc cref="IDatabaseInitializer.CreateDatabaseGetConnectionString{TDbContext}"/>
    public Task<string> CreateDatabaseGetConnectionString<TDbContext>(
        BasicDatabaseSeedingOptions<TDbContext> databaseSeeding
    ) where TDbContext : DbContext
    {
        string lastMigrationName = ContextHelper.GetLastMigrationName<TDbContext>();

        return CreateDatabaseGetConnectionStringAdvanced(
            databaseSeeding?.Name
                + nameof(CreateDatabaseGetConnectionString)
                + lastMigrationName
                + typeof(TDbContext).Assembly.FullName,
            async (connectionString) =>
            {
                await using var dbContext = ContextHelper.CreateDbContext<TDbContext>(
                    useProvider: this,
                    connectionString: connectionString,
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
        BasicDatabaseSeedingOptions<TDbContext> databaseSeeding = null
    ) where TDbContext : DbContext
    {
        return TaskUtils.RunSynchronously(() => CreateDatabaseGetConnectionString(databaseSeeding));
    }

    /// <inheritdoc cref="IDatabaseInitializer.CreateDbContextOptionsBuilder{TDbContext}"/>
    public virtual DbContextOptionsBuilder<TDbContext> CreateDbContextOptionsBuilder<TDbContext>(
        string connectionString
    ) where TDbContext : DbContext
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
        BasicDatabaseSeedingOptions<TDbContext> seedingOptions
    ) where TDbContext : DbContext
    {
        var connectionString = await CreateDatabaseGetConnectionString(seedingOptions);
        return CreateDbContextOptionsBuilder<TDbContext>(connectionString);
    }

    /// <inheritdoc cref="IDatabaseInitializer.CreateDatabaseGetDbContextOptionsBuilderSync{TDbContext}"/>
    public virtual DbContextOptionsBuilder<TDbContext> CreateDatabaseGetDbContextOptionsBuilderSync<TDbContext>(
        BasicDatabaseSeedingOptions<TDbContext> seedingOptions = null
    ) where TDbContext : DbContext
    {
        return TaskUtils.RunSynchronously(
            () => CreateDatabaseGetDbContextOptionsBuilder(seedingOptions)
        );
    }

    protected abstract void PerformBasicSeedingOperations(DbContext dbContext);

    public virtual void Dispose() { }
}
