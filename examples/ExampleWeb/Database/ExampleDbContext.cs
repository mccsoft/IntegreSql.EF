using Microsoft.EntityFrameworkCore;

namespace ExampleWeb;

public class ExampleDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public ExampleDbContext(DbContextOptions<ExampleDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(e =>
        {
            e.HasData(new() { Id = 1, Name = "John" }, new() { Id = 2, Name = "Bill" });
        });
    }
}
