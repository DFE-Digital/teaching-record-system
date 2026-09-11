using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250804.WebhookData;
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
}
