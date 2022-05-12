# IntegreSql.EF
[![example workflow](https://github.com/mcctomsk/IntegreSql.EF/actions/workflows/dotnet.yml/badge.svg)](https://github.com/mcctomsk/IntegreSql.EF/actions/workflows/dotnet.yml)
[![NUGET](https://badge.fury.io/nu/MccSoft.IntegreSql.EF.svg)](https://www.nuget.org/packages/MccSoft.IntegreSql.EF/)
[![MIT](https://img.shields.io/dub/l/vibe-d.svg)](https://opensource.org/licenses/MIT)
[![NET6](https://img.shields.io/badge/-.NET%206.0-blueviolet)](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

Provides an infrastructure to easily write **FAST** integration and unit tests using **REAL** databases  in ASP.Net Core.
Powered by the greatest [IntegreSQL](https://github.com/allaboutapps/integresql#integresql).

## Intro
I assume, you are using EFCore and PostgreSQL in your project. If you don't, you don't need the library, so stop reading now :)

There are few approaches for 'mocking' the database in autotests.
1. Use Repository pattern and just mock the repository layer in tests. According to my personal experience this is quite outdated and noone actually does this anymore :) 
2. The proven approach now is to use [EFCore with different database providers](https://docs.microsoft.com/en-us/ef/core/testing/).
    1. InMemory Database Provider. Fast, maintained by EF, but is not nearly similar to PostgreSQL, as it's even non-relational. There are **A LOT** of differences compared to real DB, so going this route is not recommended and discouraged even by Microsoft:
       >  While in-memory can work for simple, constrained query scenarios, it is highly limited and we discourage its use.
    2. Using Sqlite with filesystem DB or in-memory mode. Works ok, if you don't use any of PostgreSQL specifics (like jsonb or specific functions). Easy to set up and fast. I could recommend it (and this library supports it :)), considering the mentioned limitations.
    3. Using real PostgreSQL instance. Gives the best confidence that your code works and has all features of PostgreSQL :) Unfortunately, it comes at a cost of being quite slow (database creation takes seconds and so does data seeding).  

IntegreSQL.EF allows you to use real PostgreSQL instances, and keep the timing under 100ms per test (again, thanks to the [IntegreSQL](https://github.com/allaboutapps/integresql) project). 

## How to use it
Check out example [IntegrationTest]() (i.e using in-memory [TestServer and doing API calls](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0))

**Pre-requisite**: copy the contents of [scripts]() folder to your repo and run `docker-compose run -d` from that folder.
This will run IntegreSQL (on port 5000) and PostgreSQL (on port 5434) docker containers. 

**Important**: if you run tests as part of CI, don't forget to run the same script as part of your CI.

### Integration tests
### Unit tests
Check out [simplified example](tests/ExampleWeb.UnitTests/UnitTestSimplified.cs).

Step-by-step guide, which will show how to create a DbContext pointing to the test database:
1. In test constructor:
   1. Create database initializer of choice (you could use `SqliteDatabaseInitializer` if you'd like to):
       ```csharp
       _databaseInitializer = new NpgsqlDatabaseInitializer(
          // This is needed if you run tests NOT inside the container.
          // 5434 is the public port number of Postgresql instance
          connectionStringOverride: new() { Host = "localhost", Port = 5434 }
        )
      ```
   2. Create a new database to be used in test (database will be created using `dbContext.Database.EnsureCreated()` method): 
       ```csharp
       var connectionString = _databaseInitializer.CreateDatabaseGetConnectionStringSync<ExampleDbContext>();
      ```
   3. Create a `DatabaseContextOptions` pointing to created database. You will later use this instance to create DbContexts.
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
      public ExampleDbContext CreateDbContext()
      {
         return new ExampleDbContext(_dbContextOptions);
      }
      ```
There's also a bit more [advanced example](tests/ExampleWeb.UnitTests/UnitTestBase.cs), which allows to choose Sqlite/Postgres/No database per test.
