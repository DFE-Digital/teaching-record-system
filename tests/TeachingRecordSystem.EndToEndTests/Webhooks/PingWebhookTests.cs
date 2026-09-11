using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.EndToEndTests.Webhooks;

public class PingWebhookTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Ping_IsDeliveredToWebhookEndpoint()
    {
        // Act
        await SendPingMessageAsync();

        // Assert
        await WebhookMessageRecorder.AssertMessagesReceivedAsync(
            async request =>
            {
                Assert.Equal("ping", request.Headers.GetValues("ce-type").Single());

                var body = await request.Content!.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(body);
                Assert.True(doc.RootElement.TryGetProperty("pingId", out _));
            });
    }

    // Creates a ping message for the seeded webhook receiver endpoint and drives it through the real
    // WebhookDeliveryService queue-processing logic, so the ping message follows exactly the same path
    // it would in the Worker app, up to and including delivery to the receiving endpoint.
    private async Task SendPingMessageAsync()
    {
        using var scope = HostFixture.ApiHostServices.CreateScope();

        var webhookMessageFactory = scope.ServiceProvider.GetRequiredService<WebhookMessageFactory>();
        await webhookMessageFactory.CreatePingMessageAsync(HostFixture.WebhookEndpointId);

        var webhookDeliveryService = scope.ServiceProvider.GetRequiredService<WebhookDeliveryService>();

        WebhookDeliveryService.SendMessagesResult result;
        do
        {
            result = await webhookDeliveryService.SendMessagesAsync();
        }
        while (result.MoreRecords);
    }
}
