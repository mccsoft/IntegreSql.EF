using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Xunit;

namespace ExampleWeb.IntegrationTests;

public class PostgresWithoutEnsureCreatedTests : IntegrationTestBaseWithoutEnsureCreated
{
    public PostgresWithoutEnsureCreatedTests() : base(DatabaseType.Postgres) { }

    [Fact]
    public async Task CheckDatabaseType()
    {
        var result = await _httpClient.GetAsync("/database-type");
        var stringResult = await result.Content.ReadAsStringAsync();
        Assert.Equal("postgres", stringResult);
    }

    /// <summary>
    /// We run the test several times just to show how fast the subsequent runs are
    /// (the very first test is usually not that fast)
    /// </summary>
    [Theory()]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Test1(int iteration)
    {
        var result = await _httpClient.GetFromJsonAsync<List<UserDto>>("/users");
        Assert.Equal(
            new List<UserDto>() { new UserDto(1, "John"), new UserDto(2, "Bill"), },
            result
        );
    }

    /// <summary>
    /// We run the test several times just to show how fast the subsequent runs are
    /// (the very first test is usually not that fast)
    /// </summary>
    [Theory()]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Test2(int iteration)
    {
        var result = await _httpClient.GetFromJsonAsync<List<string>>("/users-from-service");
        Assert.Equal(new string[] { "John", "Bill", }, result);
    }
}
