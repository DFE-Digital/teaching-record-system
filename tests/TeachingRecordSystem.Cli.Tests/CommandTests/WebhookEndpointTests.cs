using System.CommandLine;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class WebhookEndpointTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task Create_PublishesWebhookEndpointCreatedEventInACreatingProcess()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var address = $"https://webhooks.example.com/{Guid.NewGuid()}";

        var command = GetSubcommand("create");
        var parseResult = command.Parse($"--user-id {applicationUser.UserId} --address {address} --cloud-event-types Alert.Created --api-version 20240920");

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);

        var endpoint = await WithDbContextAsync(async dbContext =>
            await dbContext.WebhookEndpoints.SingleAsync(e => e.Address == address));

        var (process, processEvent) = await GetProcessAndEventAsync(endpoint.WebhookEndpointId);

        Assert.Equal(ProcessType.WebhookEndpointCreating, process.ProcessType);
        Assert.Equal(SystemUser.SystemUserId, process.UserId);
        Assert.Equal(nameof(WebhookEndpointCreatedEvent), processEvent.EventName);

        var createdEvent = Assert.IsType<WebhookEndpointCreatedEvent>(processEvent.Payload);
        Assert.Equal(endpoint.WebhookEndpointId, createdEvent.WebhookEndpoint.WebhookEndpointId);
        Assert.Equal(applicationUser.UserId, createdEvent.WebhookEndpoint.ApplicationUserId);
        Assert.Equal(address, createdEvent.WebhookEndpoint.Address);
        Assert.Equal("20240920", createdEvent.WebhookEndpoint.ApiVersion);
        Assert.Equal("Alert.Created", Assert.Single(createdEvent.WebhookEndpoint.CloudEventTypes));
        Assert.True(createdEvent.WebhookEndpoint.Enabled);
    }

    [Fact]
    public async Task Update_PublishesWebhookEndpointUpdatedEventInAnUpdatingProcess()
    {
        // Arrange
        var endpoint = await CreateWebhookEndpointAsync();
        var newAddress = $"https://webhooks.example.com/{Guid.NewGuid()}";

        var command = GetSubcommand("update");
        var parseResult = command.Parse($"--id {endpoint.WebhookEndpointId} --address {newAddress}");

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);

        var (process, processEvent) = await GetProcessAndEventAsync(endpoint.WebhookEndpointId);

        Assert.Equal(ProcessType.WebhookEndpointUpdating, process.ProcessType);
        Assert.Equal(nameof(WebhookEndpointUpdatedEvent), processEvent.EventName);

        var updatedEvent = Assert.IsType<WebhookEndpointUpdatedEvent>(processEvent.Payload);
        Assert.Equal(endpoint.WebhookEndpointId, updatedEvent.WebhookEndpoint.WebhookEndpointId);
        Assert.Equal(newAddress, updatedEvent.WebhookEndpoint.Address);
        Assert.Equal(WebhookEndpointUpdatedEventChanges.Address | WebhookEndpointUpdatedEventChanges.Enabled, updatedEvent.Changes);
    }

    [Fact]
    public async Task Update_Disable_PublishesWebhookEndpointUpdatedEventWithEnabledChange()
    {
        // Arrange
        var endpoint = await CreateWebhookEndpointAsync();

        var command = GetSubcommand("update");
        var parseResult = command.Parse($"--id {endpoint.WebhookEndpointId} --enabled false");

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);

        var (process, processEvent) = await GetProcessAndEventAsync(endpoint.WebhookEndpointId);
        Assert.Equal(ProcessType.WebhookEndpointUpdating, process.ProcessType);

        var updatedEvent = Assert.IsType<WebhookEndpointUpdatedEvent>(processEvent.Payload);
        Assert.False(updatedEvent.WebhookEndpoint.Enabled);
        Assert.Equal(WebhookEndpointUpdatedEventChanges.Enabled, updatedEvent.Changes);
    }

    [Fact]
    public async Task Delete_PublishesWebhookEndpointDeletedEventInADeletingProcess()
    {
        // Arrange
        var endpoint = await CreateWebhookEndpointAsync();

        var command = GetSubcommand("delete");
        var parseResult = command.Parse($"--id {endpoint.WebhookEndpointId}");

        // Act
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);

        var deletedEndpoint = await WithDbContextAsync(async dbContext =>
            await dbContext.WebhookEndpoints
                .IgnoreQueryFilters()
                .SingleAsync(e => e.WebhookEndpointId == endpoint.WebhookEndpointId));
        Assert.NotNull(deletedEndpoint.DeletedOn);

        var (process, processEvent) = await GetProcessAndEventAsync(endpoint.WebhookEndpointId);

        Assert.Equal(ProcessType.WebhookEndpointDeleting, process.ProcessType);
        Assert.Equal(nameof(WebhookEndpointDeletedEvent), processEvent.EventName);

        var deletedEvent = Assert.IsType<WebhookEndpointDeletedEvent>(processEvent.Payload);
        Assert.Equal(endpoint.WebhookEndpointId, deletedEvent.WebhookEndpoint.WebhookEndpointId);
    }

    private Command GetSubcommand(string name) =>
        Commands.CreateWebhookEndpointCommand(Configuration).Subcommands.Single(c => c.Name == name);

    private async Task<WebhookEndpoint> CreateWebhookEndpointAsync()
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var now = DateTime.UtcNow;

        var endpoint = new WebhookEndpoint
        {
            WebhookEndpointId = Guid.NewGuid(),
            ApplicationUserId = applicationUser.UserId,
            Address = $"https://webhooks.example.com/{Guid.NewGuid()}",
            ApiVersion = "20240920",
            CloudEventTypes = ["Alert.Created"],
            Enabled = true,
            CreatedOn = now,
            UpdatedOn = now
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.WebhookEndpoints.Add(endpoint);
            await dbContext.SaveChangesAsync();
        });

        return endpoint;
    }

    // Finds the single process event raised for the given endpoint, along with the process it belongs to.
    private Task<(Core.DataStore.Postgres.Models.Process Process, ProcessEvent ProcessEvent)> GetProcessAndEventAsync(Guid webhookEndpointId) =>
        WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.EventName == nameof(WebhookEndpointCreatedEvent) ||
                    pe.EventName == nameof(WebhookEndpointUpdatedEvent) ||
                    pe.EventName == nameof(WebhookEndpointDeletedEvent))
                .ToListAsync();

            var processEvent = Assert.Single(processEvents, pe => GetWebhookEndpointId(pe.Payload) == webhookEndpointId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);

            return (process, processEvent);
        });

    private static Guid GetWebhookEndpointId(IEvent @event) => @event switch
    {
        WebhookEndpointCreatedEvent e => e.WebhookEndpoint.WebhookEndpointId,
        WebhookEndpointUpdatedEvent e => e.WebhookEndpoint.WebhookEndpointId,
        WebhookEndpointDeletedEvent e => e.WebhookEndpoint.WebhookEndpointId,
        _ => throw new ArgumentException($"Unexpected event type: {@event.GetType().Name}.", nameof(@event))
    };
}
