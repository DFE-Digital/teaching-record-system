using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Tests.ApiSchema;

public abstract class EventMapperTestBase(EventMapperFixture fixture)
{
    public TestableClock Clock => fixture.Clock;

    public DbFixture DbFixture => fixture.DbFixture;

    public ReferenceDataCache ReferenceDataCache => fixture.ReferenceDataCache;

    public TestData TestData => fixture.TestData;

    public async Task WithEventMapper<TMapper>(Func<TMapper, Task> action)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var mapper = ActivatorUtilities.CreateInstance<TMapper>(scope.ServiceProvider);
        await action(mapper);
    }
}

public class EventMapperFixture
{
    public EventMapperFixture(
        DbFixture dbFixture,
        FakeTrnGenerator trnGenerator,
        IServiceProvider serviceProvider)
    {
        Clock = new TestableClock();
        DbFixture = dbFixture;
        ReferenceDataCache = new ReferenceDataCache(dbFixture.GetDbContextFactory());

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            ReferenceDataCache,
            Clock,
            trnGenerator);

        Services = serviceProvider;
    }

    public TestableClock Clock { get; }

    public DbFixture DbFixture { get; }

    public ReferenceDataCache ReferenceDataCache { get; set; }

    public TestData TestData { get; }

    public IServiceProvider Services { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();
}
