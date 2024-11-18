using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core.Tests.Services.Webhooks;

public class WebhookSenderTests(WebhookReceiver receiver)
{
    [Fact]
    public async Task SendMessageAsync_SendsMessageWithExpectedContent()
    {
        // Arrange
        var webhookMessageId = Guid.NewGuid();
        var webhookEndpointId = Guid.NewGuid();
        var applicationUserId = Guid.NewGuid();
        var apiVersion = "TestVersion";
        var cloudEventType = "test.event";
        var cloudEventId = Guid.NewGuid().ToString();

        var data = new
        {
            Foo = 42
        };
        var serializedData = JsonSerializer.SerializeToElement(data);

        var message = new WebhookMessage()
        {
            WebhookMessageId = webhookMessageId,
            WebhookEndpointId = webhookEndpointId,
            WebhookEndpoint = new()
            {
                WebhookEndpointId = webhookEndpointId,
                ApplicationUserId = applicationUserId,
                Address = receiver.Server.BaseAddress + WebhookReceiver.Endpoint.TrimStart('/'),
                ApiVersion = apiVersion,
                CloudEventTypes = [cloudEventType],
                Enabled = true
            },
            CloudEventId = cloudEventId,
            CloudEventType = cloudEventType,
            Timestamp = DateTimeOffset.UtcNow,
            ApiVersion = apiVersion,
            Data = serializedData,
            NextDeliveryAttempt = DateTime.UtcNow
        };

        using var httpClient = receiver.CreateClient();

        var options = Options.Create(new WebhookOptions()
        {
            CanonicalDomain = "https://dummy"
        });

        var sender = new WebhookSender(httpClient, options);

        // Act
        await sender.SendMessageAsync(message);

        // Assert
        receiver.WebhookMessageRecorder.AssertMessagesReceived(
            req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Equal(WebhookReceiver.Endpoint, req.RequestUri?.LocalPath);

                Assert.Equal("1.0", req.Headers.GetValues("ce-specversion").SingleOrDefault());
                Assert.Equal(cloudEventId, req.Headers.GetValues("ce-id").SingleOrDefault());
                Assert.Equal(options.Value.CanonicalDomain, req.Headers.GetValues("ce-source").SingleOrDefault());
                Assert.Equal(cloudEventType, req.Headers.GetValues("ce-type").SingleOrDefault());
                Assert.Equal($"{options.Value.CanonicalDomain}/swagger/v3_{apiVersion}.json", req.Headers.GetValues("ce-dataschema").SingleOrDefault());
                Assert.Equal(message.Timestamp, DateTime.Parse(req.Headers.GetValues("ce-time").Single()));
                Assert.Equal("application/json; charset=utf-8", req.Content?.Headers.ContentType?.ToString());

#pragma warning disable xUnit1031 // Do not use blocking task operations in test method - we know req.Content is a MemoryStream
                var body = req.Content!.ReadAsStringAsync().GetAwaiter()!.GetResult();
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method

                AssertEx.JsonEquals(serializedData.ToString(), body);
            });
    }
}

public class WebhookMessageRecorder
{
    private readonly List<HttpRequestMessage> _messages = [];

    public void Clear() => _messages.Clear();

    public void OnRequestReceived(HttpRequestMessage request)
    {
        _messages.Add(request);
    }

    public void AssertMessagesReceived(params Action<HttpRequestMessage>[] messageInspectors) =>
        Assert.Collection(_messages, messageInspectors);
}

public sealed class WebhookReceiver : IDisposable
{
    public const string Endpoint = "/webhook";

    private readonly TestServer _server;

    public WebhookReceiver()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Services.AddSingleton<WebhookMessageRecorder>();

        var app = builder.Build();

        app.MapPost(Endpoint, async ctx =>
        {
            var messageRecorder = ctx.RequestServices.GetRequiredService<WebhookMessageRecorder>();

            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(ctx.Request.Method),
                RequestUri = new Uri(ctx.Request.GetEncodedUrl())
            };

            var memoryStream = new MemoryStream();
            await ctx.Request.Body.CopyToAsync(memoryStream);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            request.Content = new StreamContent(memoryStream);

            foreach (var header in ctx.Request.Headers)
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, [header.Value]))
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key, [header.Value]);
                }
            }

            messageRecorder.OnRequestReceived(request);

            ctx.Response.StatusCode = 204;
        });

        app.Start();

        _server = app.GetTestServer();
    }

    public IServiceProvider Services => _server.Services;

    public TestServer Server => _server;

    public WebhookMessageRecorder WebhookMessageRecorder => Services.GetRequiredService<WebhookMessageRecorder>();

    public HttpClient CreateClient() => _server.CreateClient();

    public void Dispose() => _server.Dispose();
}
