using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSql.EF.DatabaseInitialization;

public interface IUseProvider
{
    void UseProvider(DbContextOptionsBuilder options, string connectionString);
}
