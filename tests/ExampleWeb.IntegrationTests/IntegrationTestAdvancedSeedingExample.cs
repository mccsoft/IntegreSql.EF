using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExampleWeb;

public class IntegrationTestAdvancedSeedingExample
{
    protected HttpClient _httpClient;
    private readonly IDatabaseInitializer _databaseInitializer;

    public IntegrationTestAdvancedSeedingExample()
    {
        _databaseInitializer = new NpgsqlDatabaseInitializer(
            // This is needed if you run tests NOT inside the container.
            // 5434 is the public port number of Postgresql instance
            connectionStringOverride: new() { Host = "localhost", Port = 5434 }
        );
        var connectionString = _databaseInitializer.CreateDatabaseGetConnectionStringSync(
            new DatabaseSeedingOptions<ExampleDbContext>(
                Name: nameof(IntegrationTestAdvancedSeedingExample),
                SeedingFunction: async (dbContext) =>
                {
                    CreateWebApplicationFactory(dbContext.Database.GetConnectionString()!);
                    await SeedData();
                },
                DisableEnsureCreated: true
            )
        );

        CreateWebApplicationFactory(connectionString);
    }

    private async Task SeedData()
    {
        await _httpClient.PostAsJsonAsync("/users", new { Name = "qwe" });
    }

    private WebApplicationFactory<Program> CreateWebApplicationFactory(string connectionString)
    {
        var webAppFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureServices(
                    services =>
                    {
                        var descriptor = services.Single(
                            d => d.ServiceType == typeof(DbContextOptions<ExampleDbContext>)
                        );
                        services.Remove(descriptor);

                        services.AddDbContext<ExampleDbContext>(
                            options => _databaseInitializer.UseProvider(options, connectionString)
                        );
                    }
                );
            }
        );
        _httpClient = webAppFactory.CreateDefaultClient();

        return webAppFactory;
    }

    [Fact]
    public async Task Test()
    {
        var result = await _httpClient.GetFromJsonAsync<List<string>>("/users-from-service");
        Assert.Equal(new string[] { "John", "Bill", "qwe" }, result);
    }
}
