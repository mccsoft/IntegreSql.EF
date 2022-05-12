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
