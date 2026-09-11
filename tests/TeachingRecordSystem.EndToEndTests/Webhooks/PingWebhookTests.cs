namespace TeachingRecordSystem.EndToEndTests.Webhooks;

public class PingWebhookTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Ping_IsDeliveredToWebhookEndpoint()
    {
        // Act
        await HostFixture.SendPingMessageAsync();

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
}
