using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ExampleWebPostgresSpecific.Database;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExampleWeb;

public class IntegrationTestSimplified : IDisposable
{
    protected readonly HttpClient _httpClient;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly string _connectionString;

    public IntegrationTestSimplified()
    {
        // Create a database initializer of choice:
        _databaseInitializer = new NpgsqlDatabaseInitializer(
            // This is needed if you run tests NOT inside the container.
            // 5434 is the public port number of Postgresql instance
            connectionStringOverride: new() { Host = "localhost", Port = 5434, }
        );

        // Create template database (using EnsureCreated()) and a copy of it to be used in the test
        _connectionString = _databaseInitializer.CreateDatabaseGetConnectionStringSync(
            new DatabaseSeedingOptions<ExamplePostgresSpecificDbContext>(Name: "Integration")
        );

        // Create a standard WebApplicationFactory to set up web app in tests
        var webAppFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Inject 'DisableSeed' configuration variable to disable running Migrations in Startup
            builder.ConfigureAppConfiguration(
                (context, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new KeyValuePair<string, string>[] { new("DisableSeed", "true") }
                    );
                }
            );

            // Adjust DI configurations with test-specifics
            builder.ConfigureServices(services =>
            {
                // Remove default DbContext registration from DI
                var descriptor = services.Single(d =>
                    d.ServiceType == typeof(DbContextOptions<ExamplePostgresSpecificDbContext>)
                );
                services.Remove(descriptor);

                // Add new DbContext registration
                services.AddDbContext<ExamplePostgresSpecificDbContext>(options =>
                    _databaseInitializer.UseProvider(options, _connectionString)
                );
            });
        });

        // Create http client to connect to our TestServer within test
        _httpClient = webAppFactory.CreateDefaultClient();
    }

    /// <summary>
    /// We run the test several times just to show how fast the subsequent runs are
    /// (the very first test is usually not that fast)
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
#pragma warning disable xUnit1026
    public async Task Test(int iteration)
#pragma warning restore xUnit1026
    {
        var result = await _httpClient.GetFromJsonAsync<List<string>>("/users-from-service");
        Assert.Equal(new string[] { "John", "Bill", }, result);
    }

    public void Dispose()
    {
        _databaseInitializer?.RemoveDatabase(_connectionString);
    }
}
