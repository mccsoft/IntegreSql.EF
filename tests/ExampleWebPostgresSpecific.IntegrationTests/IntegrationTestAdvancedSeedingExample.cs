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
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExampleWeb;

public class IntegrationTestAdvancedSeedingExample : IDisposable
{
    protected HttpClient _httpClient = null!;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly string _connectionString;

    public IntegrationTestAdvancedSeedingExample()
    {
        _databaseInitializer = new NpgsqlDatabaseInitializer(
            // This is needed if you run tests NOT inside the container.
            // 5434 is the public port number of Postgresql instance
            connectionStringOverride: new() { Host = "localhost", Port = 5434 }
        );
        _connectionString = _databaseInitializer.CreateDatabaseGetConnectionStringSync(
            new DatabaseSeedingOptions<ExamplePostgresSpecificDbContext>(
                Name: nameof(IntegrationTestAdvancedSeedingExample),
                SeedingFunction: async (dbContext) =>
                {
                    CreateWebApplication(dbContext.Database.GetConnectionString()!);
                    await SeedData();
                },
                DisableEnsureCreated: true
            )
        );

        CreateWebApplication(_connectionString);
    }

    private async Task SeedData()
    {
        await _httpClient.PostAsJsonAsync("/users", new { Name = "qwe" });
    }

    private WebApplicationFactory<Program> CreateWebApplication(string connectionString)
    {
        var webAppFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.Single(d =>
                    d.ServiceType == typeof(DbContextOptions<ExamplePostgresSpecificDbContext>)
                );
                services.Remove(descriptor);

                services.AddDbContext<ExamplePostgresSpecificDbContext>(options =>
                    _databaseInitializer.UseProvider(options, connectionString)
                );
            });
        });
        _httpClient = webAppFactory.CreateDefaultClient();

        return webAppFactory;
    }

    [Fact]
    public async Task Test()
    {
        var result = await _httpClient.GetFromJsonAsync<List<string>>("/users-from-service");
        Assert.Equal(new string[] { "John", "Bill", "Ilon", "qwe" }, result);
    }

    public void Dispose()
    {
        _databaseInitializer?.RemoveDatabase(_connectionString);
    }
}
