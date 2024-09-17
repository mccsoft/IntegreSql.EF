using ExampleWebPostgresSpecific.Database;
using Microsoft.EntityFrameworkCore;

namespace ExampleWebPostgresSpecific;

public class UserService
{
    private readonly ExamplePostgresSpecificDbContext _postgresSpecificDbContext;

    public UserService(ExamplePostgresSpecificDbContext postgresSpecificDbContext)
    {
        _postgresSpecificDbContext = postgresSpecificDbContext;
    }

    public async Task<List<string>> GetUsers()
    {
        return await _postgresSpecificDbContext
            .Users.OrderBy(x => x.Id)
            .Select(x => x.Name)
            .ToListAsync();
    }

    public async Task AddUserWithDocuments(string name, List<Document> documents)
    {
        _postgresSpecificDbContext.Users.Add(new User() { Name = name, Documents = documents });
        await _postgresSpecificDbContext.SaveChangesAsync();
    }

    public async Task<List<User>> GetUsersWithDocuments()
    {
        return await _postgresSpecificDbContext
            .Users.Where(u => u.Documents != null && u.Documents.Count > 0)
            .ToListAsync();
    }
}
