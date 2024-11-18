using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Processing;

namespace TeachingRecordSystem.Core.Tests.Events.Processing;

[Collection(nameof(DisableParallelization))]
public class PublishEventsBackgroundServiceTests(DbFixture dbFixture) : IAsyncLifetime
{
    private readonly DbFixture _dbFixture = dbFixture;

    public Task InitializeAsync() =>
        _dbFixture.WithDbContextAsync(dbContext => dbContext.Database.ExecuteSqlAsync($"delete from events"));

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task PublishEvents_PublishesUnpublishEventsAndSetsPublishedFlag()
    {
        // Arrange
        using var dbContext = _dbFixture.GetDbContext();
        var @event = CreateDummyEvent();
        dbContext.AddEvent(@event);
        await dbContext.SaveChangesAsync();
        var dbEvent = dbContext.ChangeTracker.Entries<Event>().Last().Entity;

        var eventPublisher = new TestableEventPublisher();

        var logger = new NullLogger<PublishEventsBackgroundService>();

        var service = new PublishEventsBackgroundService(eventPublisher, _dbFixture.GetDbContextFactory(), logger);

        // Act
        await service.PublishEventsAsync(CancellationToken.None);

        // Assert
        Assert.Collection(eventPublisher.Events, e => e.Equals(@event));

        await dbContext.Entry(dbEvent).ReloadAsync();
        Assert.True(dbEvent.Published);
    }

    [Fact]
    public async Task PublishEvents_DoesNotPublishAlreadyPublishedEvent()
    {
        // Arrange
        using var dbContext = _dbFixture.GetDbContext();
        var @event = CreateDummyEvent();
        dbContext.AddEvent(@event);
        dbContext.ChangeTracker.Entries<Event>().Last().Entity.Published = true;
        await dbContext.SaveChangesAsync();

        var eventPublisher = new TestableEventPublisher();

        var logger = new NullLogger<PublishEventsBackgroundService>();

        var service = new PublishEventsBackgroundService(eventPublisher, _dbFixture.GetDbContextFactory(), logger);

        // Act
        await service.PublishEventsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(eventPublisher.Events);
    }

    [Fact]
    public async Task PublishEvents_EventPublisherThrows_DoesNotThrow()
    {
        // Arrange
        using var dbContext = _dbFixture.GetDbContext();
        var @event = CreateDummyEvent();
        dbContext.AddEvent(@event);
        await dbContext.SaveChangesAsync();
        var dbEvent = dbContext.ChangeTracker.Entries<Event>().Last().Entity;

        var eventPublisher = new Mock<IEventPublisher>();
        var publishException = new Exception("Bang!");
        eventPublisher.Setup(mock => mock.PublishEventAsync(It.IsAny<EventBase>())).ThrowsAsync(publishException);

        var logger = new Mock<ILogger<PublishEventsBackgroundService>>();

        var service = new PublishEventsBackgroundService(eventPublisher.Object, _dbFixture.GetDbContextFactory(), logger.Object);

        // Act
        await service.PublishEventsAsync(CancellationToken.None);

        // Assert
        // Can't easily assert that logging has happened here due to extension methods
    }

    private EventBase CreateDummyEvent() => new DummyEvent()
    {
        EventId = Guid.NewGuid(),
        CreatedUtc = TestableClock.Initial.ToUniversalTime(),
        RaisedBy = SystemUser.SystemUserId
    };

    private class TestableEventPublisher : IEventPublisher
    {
        private readonly List<EventBase> _events = new();

        public IReadOnlyCollection<EventBase> Events => _events.AsReadOnly();

        public Task PublishEventAsync(EventBase @event)
        {
            _events.Add(@event);
            return Task.CompletedTask;
        }
    }
}
