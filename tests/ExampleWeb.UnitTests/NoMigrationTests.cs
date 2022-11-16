using System.Linq;
using System.Threading.Tasks;
using MccSoft.IntegreSql.EF;
using MccSoft.IntegreSql.EF.DatabaseInitialization;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExampleWeb.UnitTests;

public class NoMigrationTests
{
    public class Entity1
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class MyDbContext : DbContext
    {
        public DbSet<Entity1> Entity1 { get; set; }

        public MyDbContext(DbContextOptions options) : base(options) { }
    }

    [Fact]
    public async Task NoMigrationDbContext()
    {
        var databaseInitializer = new NpgsqlDatabaseInitializer(
            // This is needed if you run tests NOT inside the container.
            // 5434 is the public port number of Postgresql instance
            connectionStringOverride: new() { Host = "localhost", Port = 5434 }
        );
        DbContextOptionsBuilder<MyDbContext>? dbContextOptionsBuilder =
            await databaseInitializer.CreateDatabaseGetDbContextOptionsBuilder<MyDbContext>(
                new DatabaseSeedingOptions<MyDbContext>("Initial")
            );
        var dbContext = new MyDbContext(dbContextOptionsBuilder.Options);
        Assert.Equal(0, dbContext.Entity1.Count());

        dbContext.Entity1.Add(new Entity1() { Title = "zxc" });
        await dbContext.SaveChangesAsync();

        Assert.Equal(1, dbContext.Entity1.Count());
    }
}
