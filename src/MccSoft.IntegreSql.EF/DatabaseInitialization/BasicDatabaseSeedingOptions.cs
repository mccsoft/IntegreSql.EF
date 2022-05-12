using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSql.EF.DatabaseInitialization;

public record BasicDatabaseSeedingOptions<TDbContext>(
    string Name,
    Func<TDbContext, Task>? SeedingFunction = null,
    Func<DbContextOptions<TDbContext>, TDbContext>? DbContextFactory = null,
    bool DisableEnsureCreated = false
) where TDbContext : DbContext;
