using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSql.EF;

/// <summary>
/// Class that initializes the template database and returns connection string to a fresh instance every time.
/// </summary>
public class SqliteDatabaseInitializer : BaseDatabaseInitializer
{
    public SqliteDatabaseInitializer() { }

    /// <summary>
    /// Stores the database initialization tasks.
    /// This is to prevent two parallel initializations of database with the same hash
    /// (if 2 tests starts in parallel).
    /// Key of dictionary is database hash, value - a Task which completes when database initialization is complete.
    /// </summary>
    private static readonly LazyConcurrentDictionary<string, Task> InitializationTasks = new();

    /// <summary>
    /// If set to true, we will MD5 the databaseHash that you provide to <see cref="CreateDatabaseGetConnectionStringInternal"/> before converting it to Sqlite filename.
    /// If false, we will use databaseHash as is.
    /// Note, that if `false` is used, there's a 200 symbols length limit for databaseHash.
    /// </summary>
    public static bool UseMd5Hash = true;

    public static bool DeleteDatabaseFilesWhenDisposed = true;

    private readonly List<string> _testDatabaseFilenames = new();

    protected override async Task<string> CreateDatabaseGetConnectionStringInternal(
        string databaseHash,
        Func<string, Task> initializeDatabase
    )
    {
        if (UseMd5Hash)
            databaseHash = Md5Hasher.CreateMD5(databaseHash);

        var templateDatabaseName = $"backup_{databaseHash}.sqlite";

        await InitializationTasks.GetOrAdd(
            databaseHash,
            async (key) =>
            {
                if (File.Exists(templateDatabaseName))
                {
                    return;
                }

                string connectionString = GetConnectionString(templateDatabaseName);
                await initializeDatabase(connectionString);
            }
        );

        var databaseFileName = $"test-{Guid.NewGuid()}.sqlite";
        File.Copy(templateDatabaseName, databaseFileName);

        _testDatabaseFilenames.Add(databaseFileName);

        return GetConnectionString(databaseFileName);
    }

    /// <summary>
    /// Returns test database to a pool
    /// </summary>
    public override Task ReturnDatabaseToPool(string connectionString)
    {
        File.Delete(connectionString);
        return Task.CompletedTask;
    }

    public override void UseProvider(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseSqlite(connectionString);
    }

    private string GetConnectionString(string fileName)
    {
        var fullPath = Path.GetFullPath(fileName);
        string connectionString = $"Data Source={fullPath};Pooling=false";
        return connectionString;
    }

    protected override void PerformBasicSeedingOperations(DbContext dbContext)
    {
        dbContext.Database.OpenConnection();
    }

    public override void Dispose()
    {
        base.Dispose();
        if (DeleteDatabaseFilesWhenDisposed)
        {
            _testDatabaseFilenames.ForEach(x => File.Delete(x));
        }
    }
}
