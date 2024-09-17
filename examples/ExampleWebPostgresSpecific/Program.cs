using ExampleWebPostgresSpecific;
using ExampleWebPostgresSpecific.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<UserService>();
builder.Services.AddDbContext<ExamplePostgresSpecificDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetValue<string>("Postgres"))
);

// builder.Services.AddDbContext<ExampleDbContext>(
//     options => options.UseSqlite(builder.Configuration.GetValue<string>("Sqlite"))
// );

var app = builder.Build();

// Not using migration in every test greatly speeds up the execution.
// So you need to make sure that database schema and seed data is cached within template database.
// (by default DB schema is created using dbContext.Database.EnsureCreated()
// if you use CreateDatabaseGetConnectionString).
if (app.Configuration.GetValue<bool>("DisableSeed") != true)
{
    await app
        .Services.CreateScope()
        .ServiceProvider.GetRequiredService<ExamplePostgresSpecificDbContext>()
        .Database.MigrateAsync();
}

app.MapGet("/", () => "Hello World!");
app.MapGet(
    "/database-type",
    (ExamplePostgresSpecificDbContext dbContext) =>
        dbContext.Database.IsNpgsql()
            ? "postgres"
            : dbContext.Database.IsSqlite()
                ? "sqlite"
                : "unknown"
);
app.MapGet(
    "/users",
    async (ExamplePostgresSpecificDbContext dbContext) =>
        await dbContext.Users.OrderBy(x => x.Id).Select(x => new { x.Id, x.Name }).ToListAsync()
);
app.MapPost(
    "/users",
    async (context) =>
    {
        var user = await context.Request.ReadFromJsonAsync<User>();
        var dbContext =
            context.RequestServices.GetRequiredService<ExamplePostgresSpecificDbContext>();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }
);
app.MapGet("/users-from-service", async (UserService userService) => await userService.GetUsers());

app.Run();
