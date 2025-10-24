using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Tests.ApiSchema;

[assembly: AssemblyFixture(typeof(EventMapperFixture))]

namespace TeachingRecordSystem.Core.Tests.ApiSchema;

public abstract class EventMapperTestBase(EventMapperFixture fixture)
{
    public TestableClock Clock => fixture.Clock;

    public IDbContextFactory<TrsDbContext> DbContextFactory => fixture.DbContextFactory;

    public ReferenceDataCache ReferenceDataCache => fixture.ReferenceDataCache;

    public TestData TestData => fixture.TestData;

    public async Task WithEventMapper<TMapper>(Func<TMapper, Task> action)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var mapper = ActivatorUtilities.CreateInstance<TMapper>(scope.ServiceProvider);
        await action(mapper);
    }
}

public class EventMapperFixture : IDisposable
{
    public EventMapperFixture()
    {
        var services = new ServiceCollection();
        CoreFixture.AddCoreServices(services);
        services.AddSingleton<PersonInfoCache>();
        services.AddMemoryCache();
        Services = services.BuildServiceProvider();
    }

    public TestableClock Clock => Services.GetRequiredService<TestableClock>();

    public IDbContextFactory<TrsDbContext> DbContextFactory => Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public ReferenceDataCache ReferenceDataCache => Services.GetRequiredService<ReferenceDataCache>();

    public TestData TestData => Services.GetRequiredService<TestData>();

    public IServiceProvider Services { get; }

    void IDisposable.Dispose() => (Services as IDisposable)?.Dispose();
}
