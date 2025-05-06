﻿using System;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MccSoft.IntegreSql.EF.DatabaseInitialization;

public class ContextHelper
{
    /// <summary>
    /// Returns the name of the last DB migration in passed DbContext.
    /// Tries to create the DbContext using the passed factory method,
    /// or by using a constructor with DbContextOptions as a first argument and nulls as all the rest.
    /// </summary>
    public static string? GetLastMigrationName<T>(IUseProvider useProvider = null,
        Func<DbContextOptions<T>, T> factoryMethod = null)
        where T : DbContext
    {
        // Use the provided useProvider or create a default one
        useProvider ??= new NpgsqlDatabaseInitializer();

        var dbContext = CreateDbContext(useProvider, factoryMethod);
        var assemblyMigrations = dbContext.Database.GetMigrations();
        return assemblyMigrations.LastOrDefault();
    }

    internal static T CreateDbContext<T>(
        IUseProvider useProvider,
        Func<DbContextOptions<T>, T> factoryMethod = null,
        string connectionString = "Server=any"
    ) where T : DbContext
    {
        factoryMethod ??= (options) =>
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T), options);
            }
            catch (MissingMethodException)
            {
                try
                {
                    return (T)Activator.CreateInstance(typeof(T), options, null);
                }
                catch (MissingMethodException)
                {
                    return (T)Activator.CreateInstance(typeof(T), options, null, null);
                }
            }
        };

        var optionsBuilder = new DbContextOptionsBuilder<T>();
        useProvider?.UseProvider(optionsBuilder, connectionString);
        var dbContext = factoryMethod(optionsBuilder.Options);
        return dbContext;
    }

    internal static void ReloadTypesForEnumSupport(DbContext context)
    {
        var conn = (NpgsqlConnection)context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }

        conn.ReloadTypes();
    }
}