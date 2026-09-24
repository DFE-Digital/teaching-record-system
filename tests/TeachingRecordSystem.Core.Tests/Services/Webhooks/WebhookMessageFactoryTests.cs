using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.ApiSchema.V3.V20260515.WebhookData;
using TeachingRecordSystem.Core.ApiSchema.V3.V20260915.WebhookData;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core.Tests.Services.Webhooks;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class WebhookMessageFactoryTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreatePingMessageAsync_CreatesUndeliveredPingMessageForEndpoint()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var endpoint = await WithDbContextAsync(async dbContext =>
        {
            var e = new WebhookEndpoint
            {
                WebhookEndpointId = Guid.NewGuid(),
                ApplicationUserId = applicationUser.UserId,
                Address = $"https://webhooks.example.com/{Guid.NewGuid()}",
                ApiVersion = "20250804",
                CloudEventTypes = ["Alert.Created"],
                Enabled = true,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            dbContext.WebhookEndpoints.Add(e);
            await dbContext.SaveChangesAsync();
            return e;
        });

        var webhookMessageFactory = Services.GetRequiredService<WebhookMessageFactory>();

        // Act
        var message = await webhookMessageFactory.CreatePingMessageAsync(endpoint.WebhookEndpointId);

        // Assert
        Assert.Equal(endpoint.WebhookEndpointId, message.WebhookEndpointId);
        Assert.NotNull(message.WebhookEndpoint);
        Assert.Equal(endpoint.WebhookEndpointId, message.WebhookEndpoint.WebhookEndpointId);
        Assert.Equal(PingNotification.CloudEventType, message.CloudEventType);
        Assert.Equal(endpoint.ApiVersion, message.ApiVersion);
        Assert.Null(message.Delivered);
        Assert.Empty(message.DeliveryAttempts);
        Assert.Empty(message.DeliveryErrors);

        var data = message.Data.Deserialize<PingNotification>(_jsonSerializerOptions);
        Assert.NotNull(data);
        Assert.NotEqual(Guid.Empty, data.PingId);

        var savedMessage = await WithDbContextAsync(async dbContext =>
            await dbContext.WebhookMessages.SingleAsync(m => m.WebhookMessageId == message.WebhookMessageId));
        Assert.Equal(PingNotification.CloudEventType, savedMessage.CloudEventType);
    }

    [Fact]
    public async Task CreateMessagesAsync_TrnRequestCompleted_OnlyCreatesMessagesForTrnRequestApplicationUsersEndpoints()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var otherApplicationUser = await TestData.CreateApplicationUserAsync();

        var (_, trnRequestMetadata, _) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);
        var trnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata) with { Status = TrnRequestStatus.Completed };

        var @event = new TrnRequestUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SourceApplicationUserId = applicationUser.UserId,
            RequestId = trnRequest.RequestId,
            Changes = TrnRequestUpdatedChanges.Status,
            TrnRequest = trnRequest,
            OldTrnRequest = trnRequest with { Status = TrnRequestStatus.Pending },
            ReasonDetails = null
        };

        var (endpoint, _) = await WithDbContextAsync(async dbContext =>
        {
            var e = CreateTrnRequestCompletedEndpoint(applicationUser.UserId);
            var other = CreateTrnRequestCompletedEndpoint(otherApplicationUser.UserId);
            dbContext.WebhookEndpoints.AddRange(e, other);
            await dbContext.SaveChangesAsync();
            return (e, other);
        });

        Services.GetRequiredService<IMemoryCache>().Remove(CacheKeys.EnabledWebhookEndpoints());

        // Act
        var messages = await WithServiceAsync<WebhookMessageFactory, IEnumerable<WebhookMessage>>(
            factory => factory.CreateMessagesAsync(@event));

        // Assert
        var message = Assert.Single(messages);
        Assert.Equal(endpoint.WebhookEndpointId, message.WebhookEndpointId);
        Assert.Equal(TrnRequestCompletedNotification.CloudEventType, message.CloudEventType);
    }

    private static WebhookEndpoint CreateTrnRequestCompletedEndpoint(Guid applicationUserId) => new()
    {
        WebhookEndpointId = Guid.NewGuid(),
        ApplicationUserId = applicationUserId,
        Address = $"https://webhooks.example.com/{Guid.NewGuid()}",
        ApiVersion = "20260515",
        CloudEventTypes = [TrnRequestCompletedNotification.CloudEventType],
        Enabled = true,
        CreatedOn = DateTime.UtcNow,
        UpdatedOn = DateTime.UtcNow
    };
}
