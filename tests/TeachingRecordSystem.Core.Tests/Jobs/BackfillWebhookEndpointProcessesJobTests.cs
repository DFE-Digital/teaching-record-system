using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillWebhookEndpointProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyCreatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var webhookEndpoint = CreateWebhookEndpoint();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.WebhookEndpointCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            WebhookEndpoint = webhookEndpoint
        });

        // Act
        await WithServiceAsync<BackfillWebhookEndpointProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(WebhookEndpointCreatedEvent), processEvent.EventName);
            Assert.Equal(legacyEvent.CreatedUtc, processEvent.CreatedOn);

            var createdEvent = Assert.IsType<WebhookEndpointCreatedEvent>(processEvent.Payload);
            Assert.Equal(webhookEndpoint.WebhookEndpointId, createdEvent.WebhookEndpoint.WebhookEndpointId);
            Assert.Equal(webhookEndpoint.Address, createdEvent.WebhookEndpoint.Address);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.WebhookEndpointCreating, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Empty(process.PersonIds);
        });
    }

    [Fact]
    public async Task Execute_LegacyUpdatedEvent_CreatesProcessAndProcessEventWithChanges()
    {
        // Arrange
        var webhookEndpoint = CreateWebhookEndpoint();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.WebhookEndpointUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            WebhookEndpoint = webhookEndpoint,
            Changes = LegacyEvents.WebhookEndpointUpdatedChanges.Address | LegacyEvents.WebhookEndpointUpdatedChanges.Enabled
        });

        // Act
        await WithServiceAsync<BackfillWebhookEndpointProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(WebhookEndpointUpdatedEvent), processEvent.EventName);

            var updatedEvent = Assert.IsType<WebhookEndpointUpdatedEvent>(processEvent.Payload);
            Assert.Equal(webhookEndpoint.WebhookEndpointId, updatedEvent.WebhookEndpoint.WebhookEndpointId);
            Assert.Equal(
                WebhookEndpointUpdatedEventChanges.Address | WebhookEndpointUpdatedEventChanges.Enabled,
                updatedEvent.Changes);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.WebhookEndpointUpdating, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyDeletedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var webhookEndpoint = CreateWebhookEndpoint();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.WebhookEndpointDeletedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            WebhookEndpoint = webhookEndpoint
        });

        // Act
        await WithServiceAsync<BackfillWebhookEndpointProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(WebhookEndpointDeletedEvent), processEvent.EventName);

            var deletedEvent = Assert.IsType<WebhookEndpointDeletedEvent>(processEvent.Payload);
            Assert.Equal(webhookEndpoint.WebhookEndpointId, deletedEvent.WebhookEndpoint.WebhookEndpointId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.WebhookEndpointDeleting, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotBackfillTwice()
    {
        // Arrange
        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.WebhookEndpointCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            WebhookEndpoint = CreateWebhookEndpoint()
        });

        await WithServiceAsync<BackfillWebhookEndpointProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillWebhookEndpointProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes.Where(p => p.ProcessType == ProcessType.WebhookEndpointCreating).ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.WebhookEndpointCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            WebhookEndpoint = CreateWebhookEndpoint()
        });

        // Act
        await WithServiceAsync<BackfillWebhookEndpointProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventOfAnotherType_IsNotBackfilled()
    {
        // Arrange
        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.ApiKeyCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            ApiKey = new EventModels.ApiKey
            {
                ApiKeyId = Guid.NewGuid(),
                ApplicationUserId = Guid.NewGuid(),
                Key = "the-key",
                Expires = null
            }
        });

        // Act
        await WithServiceAsync<BackfillWebhookEndpointProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.WebhookEndpoint CreateWebhookEndpoint() => new()
    {
        WebhookEndpointId = Guid.NewGuid(),
        ApplicationUserId = Guid.NewGuid(),
        Address = "https://webhooks.example.com/endpoint",
        ApiVersion = "20240920",
        CloudEventTypes = ["Alert.Created"],
        Enabled = true
    };

    private async Task<TEvent> AddLegacyEventAsync<TEvent>(TEvent legacyEvent) where TEvent : LegacyEvents.EventBase
    {
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(legacyEvent);
            await dbContext.SaveChangesAsync();
        });

        return legacyEvent;
    }
}
