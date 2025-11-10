using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

[Collection(nameof(DisableParallelization))]
public abstract class CommandTestBase(IServiceProvider services)
{
    protected IClock Clock => services.GetRequiredService<IClock>();

    protected DbHelper DbHelper => services.GetRequiredService<DbHelper>();

    protected IConfiguration Configuration => services.GetRequiredService<IConfiguration>();

    protected IDbContextFactory<TrsDbContext> DbContextFactory => services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected TestData TestData => services.GetRequiredService<TestData>();

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}
