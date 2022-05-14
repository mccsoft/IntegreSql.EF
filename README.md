# IntegreSql.EF

[![example workflow](https://github.com/mcctomsk/IntegreSql.EF/actions/workflows/dotnet.yml/badge.svg)](https://github.com/mcctomsk/IntegreSql.EF/actions/workflows/dotnet.yml)
[![NUGET](https://badge.fury.io/nu/MccSoft.IntegreSql.EF.svg)](https://www.nuget.org/packages/MccSoft.IntegreSql.EF/)
[![MIT](https://img.shields.io/dub/l/vibe-d.svg)](https://opensource.org/licenses/MIT)
[![NET6](https://img.shields.io/badge/-.NET%206.0-blueviolet)](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

Provides an infrastructure to easily write **FAST** integration and unit tests using **REAL** databases in ASP.Net Core.
Powered by the greatest [IntegreSQL](https://github.com/allaboutapps/integresql#integresql).

## Intro

I assume, you are using EFCore and PostgreSQL in your project. If you don't, you don't need the library, so stop reading
now :)

There are few approaches for 'mocking' the database in autotests.

1. Use Repository pattern and just mock the repository layer in tests. According to my personal experience this is quite
   outdated and noone actually does this anymore :)
2. The proven approach now is to
   use [EFCore with different database providers](https://docs.microsoft.com/en-us/ef/core/testing/).
    1. InMemory Database Provider. Fast, maintained by EF, but is not nearly similar to PostgreSQL, as it's even
       non-relational. There are **A LOT** of differences compared to real DB, so going this route is not recommended
       and discouraged even by Microsoft:
       > While in-memory can work for simple, constrained query scenarios, it is highly limited and we discourage its
       use.
    2. Using Sqlite with filesystem DB or in-memory mode. Works ok, if you don't use any of PostgreSQL specifics (like
       jsonb or specific functions). Easy to set up and fast. I could recommend it (and this library supports it :)),
       considering the mentioned limitations.
    3. Using real PostgreSQL instance. Gives the best confidence that your code works and has all features of
       PostgreSQL :) Unfortunately, it comes at a cost of being quite slow (database creation takes seconds and so does
       data seeding).

IntegreSQL.EF allows you to use real PostgreSQL instances, and keep the timing under 100ms per test (again, thanks to
the [IntegreSQL](https://github.com/allaboutapps/integresql) project).

## How to use it
in-memory [TestServer and doing API calls](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0))

**Pre-requisite**: copy the contents of [scripts](scripts) folder to your repo and run `docker-compose run -d` from that
folder.
This will run IntegreSQL (on port 5000) and PostgreSQL (on port 5434) docker containers.

**Important**: if you run tests as part of CI, don't forget to run the same script as part of your CI.

### Unit tests

Check out [simplified example](tests/ExampleWeb.UnitTests/UnitTestSimplified.cs).

In UnitTests we usually create an instance of tested class manually. So, if tested class requires DbContext to be passed
in constructor, we need to create it within our tests somehow.

Here's a step-by-step guide, which will show how to create a DbContext pointing to the test database:

1. In test constructor:
    1. Create database initializer of choice (you could use `SqliteDatabaseInitializer` if you'd like to):
        ```csharp
        _databaseInitializer = new NpgsqlDatabaseInitializer(
           // This is needed if you run tests NOT inside the container.
           // 5434 is the public port number of Postgresql instance
           connectionStringOverride: new() { Host = "localhost", Port = 5434 }
         )
       ```
    2. Create a new database to be used in test (database will be created using `dbContext.Database.EnsureCreated()`
       method):
        ```csharp
        var connectionString = _databaseInitializer.CreateDatabaseGetConnectionStringSync<ExampleDbContext>();
       ```
    3. Create a `DatabaseContextOptions` pointing to created database. You will later use this instance to create
       DbContexts.
        ```csharp
         _options = _databaseInitializer.CreateDbContextOptionsBuilder<ExampleDbContext>(
             connectionString
         ).Options;
       ```
       Commands from step 2 and 3 could be united in
       ```csharp
       _dbContextOptions = _databaseInitializer
             .CreateDatabaseGetDbContextOptionsBuilderSync<ExampleDbContext>()
             .Options;
       ```
    4. Add a method to create a `DbContext` pointing to the newly created database:
       ```csharp
       public ExampleDbContext CreateDbContext 
       {
          return new ExampleDbContext(_dbContextOptions);
       }
       ```

There's also a bit more [advanced example](tests/ExampleWeb.UnitTests/UnitTestBase.cs), which allows to choose
Sqlite/Postgres/No database per test.

### Integration tests

Integration tests are similar to [Unit Tests](#unit-tests) above, except we need to use some `WebApplicationFactory` ceremony to
create `TestServer` and `TestClient`, and alter DI container configuration to use another DbContext connection string.

The key factor to speed up Integration tests is to prepare a template database with seeded data and disable seeding in the Startup (cause it usually takes most of the time). 

Check out [full simplified example](tests/ExampleWeb.IntegrationTests/IntegrationTestSimplified.cs) or read the comments below:
```csharp
 public IntegrationTestSimplified()
 {
     // Create a database initializer of choice:
     _databaseInitializer = new NpgsqlDatabaseInitializer(
         // This is needed if you run tests NOT inside the container.
         // 5434 is the public port number of Postgresql instance
         connectionStringOverride: new() { Host = "localhost", Port = 5434, }
     );

     // Create template database (using EnsureCreated()) and a copy of it to be used in the test
     var connectionString = _databaseInitializer.CreateDatabaseGetConnectionStringSync(
         new BasicDatabaseSeedingOptions<ExampleDbContext>(Name: "Integration")
     );

     // Create a standard WebApplicationFactory to set up web app in tests
     var webAppFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(
         builder =>
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
             builder.ConfigureServices(
                 services =>
                 {
                     // Remove default DbContext registration from DI
                     var descriptor = services.Single(
                         d => d.ServiceType == typeof(DbContextOptions<ExampleDbContext>)
                     );
                     services.Remove(descriptor);

                     // Add new DbContext registration
                     services.AddDbContext<ExampleDbContext>(
                         options => _databaseInitializer.UseProvider(options, connectionString)
                     );
                 }
             );
         }
     );
     // Create http client to connect to our TestServer within test
     _httpClient = webAppFactory.CreateDefaultClient();
 }
```
There's a bit more [advanced example](tests/ExampleWeb.IntegrationTests/IntegrationTestBase.cs) here as well.

# API
### NpgsqlDatabaseInitializer
This is the main entry point to do database caching.
#### .CreateDatabaseGetConnectionString
Creates a template database (if not created before) using `DbContext.Database.EnsureCreated`.
Accepts a `databaseSeedingOptions` parameter, which allows to configure some additional seeding (beside standard `EnsureCreated`).

Then it creates a copy of template database to be used in each test.
Returns a connection string for passed DbContext to be used in the test.

Normally you should run this function once per test.

#### .CreateDatabaseGetDbContextOptionsBuilder
Creates the database using [CreateDatabaseGetConnectionString](#.CreateDatabaseGetConnectionString) and returns a DbContextOptionsBuilder for the created database.

Returned `DbContextOptions` are meant to be stored in a Test Class field and used to create a DbContext instance (pointing to the same DB during the test).

This is a simple helper method so you don't have to create `DbContextOptions` by hand.

#### .ReturnDatabaseToPool
Returns test database to a pool (which allows consequent tests to reuse this database).

Note that you need to clean up database by yourself before returning it to the pool!
If you return a dirty database, consequent tests might fail!

If you don't want to clean it up, just don't use this function (or use `RemoveDatabase` instead).
Dirty databases are automatically deleted by IntegreSQL once database number exceeds a certain limit (500 by default).

#### .RemoveDatabase
Removes the test database.

You could do this in `Dispose` method of your test if you don't want to have postgresql databases just hanging around.

However, this is completely optional, and test databases will be deleted by IntegreSQL itself (when there's more than 500 databases).   

# Advanced
Sometimes in Integration tests you might want to setup a template database by doing API calls. In other words, you need the full-blown `TestServer` and `TestClient` to do the seeding.
Though, it's not recommended per se (as it's a bit complicated and slower than seeding via DbContext directly), there are perfectly reasonable scenarios for this case.

This library supports that and you could check out [an example](tests/ExampleWeb.IntegrationTests/IntegrationTestAdvancedSeedingExample.cs) doing just that.

P.S. The example is intentionally simplified and could be easily converted to DbContext-based seeding, and serves the demonstration purposes only.