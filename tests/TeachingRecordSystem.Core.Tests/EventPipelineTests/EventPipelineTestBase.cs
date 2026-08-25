using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.TestCommon.Database;

namespace TeachingRecordSystem.Core.Tests.EventPipelineTests;

public class EventPipelineTestBase : PooledDatabaseTestBase
{
    private readonly EventPipelineFixture _fixture;

    protected EventPipelineTestBase(EventPipelineFixture fixture)
    {
        _fixture = fixture;

        TestScopedServices.Reset();
    }

    protected FakeTimeProvider TimeProvider => TestScopedServices.GetCurrent().TimeProvider;

    protected IDbContextFactory<TrsDbContext> DbContextFactory => _fixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected EventCapture Events => _fixture.Services.GetRequiredService<EventCapture>();

    protected IEventPublisher EventPublisher => _fixture.Services.GetRequiredService<IEventPublisher>();

    protected CaptureEventObserver LegacyEventObserver => TestScopedServices.GetCurrent().LegacyEventObserver;

    protected IServiceProvider Services => _fixture.Services;

    protected TestData TestData => Services.GetRequiredService<TestData>();
}
