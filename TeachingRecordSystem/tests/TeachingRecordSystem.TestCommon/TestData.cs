using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    private static readonly object _gate = new();
    private static readonly HashSet<string> _emails = new();

    private readonly IDbContextFactory<TrsDbContext> _dbContextFactory;

    public TestData(IDbContextFactory<TrsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public string GenerateName() => Faker.Name.FullName();

    public string GenerateChangedName(string currentName)
    {
        string newName;

        do
        {
            newName = GenerateName();
        }
        while (newName == currentName);

        return newName;
    }

    public string GenerateUniqueEmail()
    {
        string email;

        lock (_gate)
        {
            do
            {
                email = Faker.Internet.Email();
            }
            while (!_emails.Add(email));
        }

        return email;
    }

    private async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    private async Task WithDbContext(Func<TrsDbContext, Task> action)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        await action(dbContext);
    }
}
