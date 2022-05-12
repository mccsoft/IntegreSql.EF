using System.Threading.Tasks;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Xunit;

namespace ExampleWeb.UnitTests;

public class SqliteTests : UnitTestBase
{
    private readonly UserService _service;

    public SqliteTests() : base(DatabaseType.Sqlite)
    {
        _service = new UserService(CreateDbContext());
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
        var users = await _service.GetUsers();
        Assert.Equal(new[] { "John", "Bill" }, users);
    }
}
