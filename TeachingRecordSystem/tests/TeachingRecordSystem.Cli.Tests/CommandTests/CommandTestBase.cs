using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public abstract class CommandTestBase(CompositionRoot compositionRoot)
{
    protected IClock Clock => compositionRoot.Services.GetRequiredService<IClock>();

    protected IConfiguration Configuration => compositionRoot.Services.GetRequiredService<IConfiguration>();

    protected IDbContextFactory<TrsDbContext> DbContextFactory => compositionRoot.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected TestData TestData => compositionRoot.Services.GetRequiredService<TestData>();

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);
}
