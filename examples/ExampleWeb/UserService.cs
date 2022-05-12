using Microsoft.EntityFrameworkCore;

namespace ExampleWeb;

public class UserService
{
    private readonly ExampleDbContext _dbContext;

    public UserService(ExampleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<string>> GetUsers()
    {
        return await _dbContext.Users.Select(x => x.Name).ToListAsync();
    }
}
