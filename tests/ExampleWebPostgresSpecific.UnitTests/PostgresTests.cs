using ExampleWebPostgresSpecific.Database;
using MccSoft.IntegreSql.EF.DatabaseInitialization;

namespace ExampleWebPostgresSpecific.UnitTests;

public class PostgresTests : UnitTestBase
{
    private readonly UserService _service;

    public PostgresTests()
        : base(DatabaseType.Postgres)
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
#pragma warning disable xUnit1026
    public async Task Test1(int iteration)
#pragma warning restore xUnit1026
    {
        var users = await _service.GetUsers();
        Assert.Equal(new[] { "John", "Bill" }, users);
    }

    [Fact]
    public async Task Test2()
    {
        var name = "Igor";
        var documents = new List<Document>
        {
            new()
            {
                Name = "Document1",
                SubDocuments = [new() { Name = "SubDocument1" }, new() { Name = "SubDocument2" }]
            },
            new() { Name = "Document2" }
        };
        await _service.AddUserWithDocuments(name, documents);
        var users = await _service.GetUsersWithDocuments();
        Assert.NotNull(users);
        Assert.Single(users);
        Assert.Contains(users, x => x.Name == name);

        Assert.Equal(documents, users.First().Documents);
    }
}
