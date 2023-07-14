using Microsoft.Extensions.Logging.Abstractions;
using TeachingRecordSystem.Api.Events.Processing;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Api.Tests.Events.Processing;

[TestClass(TestConcurrencyMode.NoConcurrency)]
[ExecuteSqlSetup($"delete from events")]
public class PublishEventsBackgroundServiceTests
{
    private readonly DbFixture _dbFixture;

    public PublishEventsBackgroundServiceTests(DbFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    [Test]
    public async Task PublishEvents_PublishesUnpublishEventsAndSetsPublishedFlag()
    {
        // Arrange
        using var dbContext = _dbFixture.GetDbContext();
        var @event = CreateDummyEvent();
        dbContext.AddEvent(@event);
        await dbContext.SaveChangesAsync();
        var dbEvent = dbContext.ChangeTracker.Entries<Event>().Last().Entity;

        var eventObserver = new TestableEventObserver();

        var logger = new NullLogger<PublishEventsBackgroundService>();

        var service = new PublishEventsBackgroundService(eventObserver, _dbFixture.GetDbContextFactory(), logger);

        // Act
        await service.PublishEvents(CancellationToken.None);

        // Assert
        Assert.Collection(eventObserver.Events, e => e.Equals(@event));

        await dbContext.Entry(dbEvent).ReloadAsync();
        Assert.True(dbEvent.Published);
    }

    [Test]
    public async Task PublishEvents_DoesNotPublishAlreadyPublishedEvent()
    {
        // Arrange
        using var dbContext = _dbFixture.GetDbContext();
        var @event = CreateDummyEvent();
        dbContext.AddEvent(@event);
        dbContext.ChangeTracker.Entries<Event>().Last().Entity.Published = true;
        await dbContext.SaveChangesAsync();

        var eventObserver = new TestableEventObserver();

        var logger = new NullLogger<PublishEventsBackgroundService>();

        var service = new PublishEventsBackgroundService(eventObserver, _dbFixture.GetDbContextFactory(), logger);

        // Act
        await service.PublishEvents(CancellationToken.None);

        // Assert
        Assert.Empty(eventObserver.Events);
    }

    [Test]
    public async Task PublishEvents_EventObserverThrows_DoesNotThrow()
    {
        // Arrange
        using var dbContext = _dbFixture.GetDbContext();
        var @event = CreateDummyEvent();
        dbContext.AddEvent(@event);
        await dbContext.SaveChangesAsync();
        var dbEvent = dbContext.ChangeTracker.Entries<Event>().Last().Entity;

        var eventObserver = new Mock<IEventObserver>();
        var publishException = new Exception("Bang!");
        eventObserver.Setup(mock => mock.OnEventSaved(It.IsAny<EventBase>())).ThrowsAsync(publishException);

        var logger = new Mock<ILogger<PublishEventsBackgroundService>>();

        var service = new PublishEventsBackgroundService(eventObserver.Object, _dbFixture.GetDbContextFactory(), logger.Object);

        // Act
        await service.PublishEvents(CancellationToken.None);

        // Assert
        // Can't easily assert that logging has happened here due to extension methods
    }

    private EventBase CreateDummyEvent() => new DummyEvent()
    {
        DummyProperty = Faker.Name.FullName(),
        CreatedUtc = TestableClock.Initial.ToUniversalTime()
    };

    private class TestableEventObserver : IEventObserver
    {
        private readonly List<EventBase> _events = new();

        public IReadOnlyCollection<EventBase> Events => _events.AsReadOnly();

        public Task OnEventSaved(EventBase @event)
        {
            _events.Add(@event);
            return Task.CompletedTask;
        }
    }
}
