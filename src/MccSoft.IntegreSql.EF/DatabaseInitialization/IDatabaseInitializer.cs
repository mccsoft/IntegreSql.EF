using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSql.EF.DatabaseInitialization;

public interface IDatabaseInitializer : IDisposable, IUseProvider
{
    /// <summary>
    /// Creates a template database (if not created before) using DbContext.Database.EnsureCreated and
    /// runs a <paramref name="databaseSeeding"/> on it.
    /// Then it creates a copy of template database to be used in each test.
    /// Returns a connection string for passed DbContext to be used in the test.
    /// Normally you should run this function once per test.
    /// </summary>
    /// <returns>Connection string to a copy of template database</returns>
    Task<string> CreateDatabaseGetConnectionString<TDbContext>(
        DatabaseSeedingOptions<TDbContext> databaseSeeding = null
    ) where TDbContext : DbContext;

    /// <summary>
    /// Creates a template database (if not created before) using DbContext.Database.EnsureCreated and
    /// runs a <paramref name="databaseSeeding"/> on it.
    /// Then it creates a copy of template database to be used in each test.
    /// Returns a connection string for passed DbContext to be used in the test.
    /// Normally you should run this function once per test.
    /// </summary>
    /// <returns>Connection string to a copy of template database</returns>
    string CreateDatabaseGetConnectionStringSync<TDbContext>(
        DatabaseSeedingOptions<TDbContext> databaseSeeding = null
    ) where TDbContext : DbContext;

    /// <summary>
    /// Creates the DbContextOptionsBuilder for passed connection string.
    /// Should be stored in a Test Class field and used to create a DbContext instance (pointing to the same DB during the test).
    /// </summary>
    DbContextOptionsBuilder<TDbContext> CreateDbContextOptionsBuilder<TDbContext>(
        string connectionString
    ) where TDbContext : DbContext;

    /// <summary>
    /// Creates the database using <see cref="CreateDatabaseGetConnectionString{TDbContext}"/>
    /// and returns a DbContextOptionsBuilder for the created database.
    /// Should be stored in a Test Class field and used to create a DbContext instance (pointing to the same DB during the test).
    /// </summary>
    Task<DbContextOptionsBuilder<TDbContext>> CreateDatabaseGetDbContextOptionsBuilder<TDbContext>(
        DatabaseSeedingOptions<TDbContext> seedingOptions = null
    ) where TDbContext : DbContext;

    /// <summary>
    /// Creates the database using <see cref="CreateDatabaseGetConnectionString{TDbContext}"/>
    /// and returns a DbContextOptionsBuilder for the created database.
    /// Should be stored in a Test Class field and used to create a DbContext instance (pointing to the same DB during the test).
    /// </summary>
    DbContextOptionsBuilder<TDbContext> CreateDatabaseGetDbContextOptionsBuilderSync<TDbContext>(
        DatabaseSeedingOptions<TDbContext> seedingOptions = null
    ) where TDbContext : DbContext;

    /// <summary>
    /// Returns test database to a pool.
    /// </summary>
    Task ReturnDatabaseToPool(string connectionString);

    /// <summary>
    /// Creates connectionString using <see cref="CreateDatabaseGetConnectionString{TDbContext}"/>.
    /// Calls options.UseNpgsql() or options.UseSqlite() depending on database provider initializer works with.
    /// Helps build the common WebApplicationFactory creation code which only requires IDatabaseInitializer
    /// </summary>
    void UseProvider<TDbContext>(
        DbContextOptionsBuilder options,
        DatabaseSeedingOptions<TDbContext> databaseSeedingOptions
    ) where TDbContext : DbContext;
}
