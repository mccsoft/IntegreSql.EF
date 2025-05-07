using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace ExampleWebPostgresSpecific.Database;

public class ExamplePostgresSpecificDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ExamplePostgresSpecificDbContext(
        DbContextOptions<ExamplePostgresSpecificDbContext> options
    )
        : base(options) { }

    public static NpgsqlDbContextOptionsBuilder MapEnums(NpgsqlDbContextOptionsBuilder builder) =>
        builder.MapEnum<UserType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(e =>
        {
            e.Property(x => x.Documents).HasColumnType("jsonb");
            e.HasData(
                new() { Id = 1, Name = "John" },
                new()
                {
                    Id = 2,
                    Name = "Bill",
                    UserType = UserType.Admin
                }
            );
        });
    }
}
