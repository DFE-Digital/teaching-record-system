using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSign.AspNetCore;
using NSign.Providers;
using NSign.Signatures;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Webhooks;
using TeachingRecordSystem.Core.Tests.Services.Webhooks;

[assembly: AssemblyFixture(typeof(WebhookReceiver))]

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
                Enabled = true,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            },
            CloudEventId = cloudEventId,
            CloudEventType = cloudEventType,
            Timestamp = DateTimeOffset.UtcNow,
            ApiVersion = apiVersion,
            Data = serializedData,
            NextDeliveryAttempt = DateTime.UtcNow
        };

        var options = receiver.GetWebhookOptions();
        var sender = receiver.GetWebhookSender();

        // Act
        await sender.SendMessageAsync(message);

        // Assert
        await receiver.WebhookMessageRecorder.AssertMessagesReceivedAsync(
            async req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Equal(WebhookReceiver.Endpoint, req.RequestUri?.LocalPath);

                Assert.Equal("1.0", req.Headers.GetValues("ce-specversion").SingleOrDefault());
                Assert.Equal(cloudEventId, req.Headers.GetValues("ce-id").SingleOrDefault());
                Assert.Equal(options.CanonicalDomain, req.Headers.GetValues("ce-source").SingleOrDefault());
                Assert.Equal(cloudEventType, req.Headers.GetValues("ce-type").SingleOrDefault());
                Assert.Equal($"{options.CanonicalDomain}/swagger/v3_{apiVersion}.json", req.Headers.GetValues("ce-dataschema").SingleOrDefault());
                Assert.Equal(message.Timestamp, DateTime.Parse(req.Headers.GetValues("ce-time").Single()));
                Assert.Equal("application/json; charset=utf-8", req.Content?.Headers.ContentType?.ToString());
                Assert.NotNull(req.Headers.GetValues("signature-input").SingleOrDefault());
                Assert.NotNull(req.Headers.GetValues("signature").SingleOrDefault());

                var body = await req.Content!.ReadAsStringAsync();
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

    public Task AssertMessagesReceivedAsync(params Func<HttpRequestMessage, Task>[] messageInspectors) =>
        Assert.CollectionAsync(_messages, messageInspectors);
}

public sealed class WebhookReceiver : IDisposable
{
    public const string Endpoint = "/webhook";

    private readonly TestServer _server;

    public WebhookReceiver()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        var sigingKey = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var certRequest = new CertificateRequest("CN=Tests", sigingKey, HashAlgorithmName.SHA384);
        var certificate = certRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));
        var certificatePem = certificate.ExportCertificatePem();
        var keyPem = sigingKey.ExportECPrivateKeyPem();

        builder.Services.Configure<WebhookOptions>(options =>
        {
            options.CanonicalDomain = "https://dummy";
            options.SigningKeyId = "key";
            options.Keys =
            [
                new WebhookOptionsKey()
                {
                    KeyId = "key",
                    CertificatePem = certificatePem,
                    PrivateKeyPem = keyPem
                }
            ];
        });

        builder.Services.AddSingleton<WebhookMessageRecorder>();
        WebhookSender.Register(builder.Services, () => _server!.CreateHandler());

        builder.Services.Configure<RequestSignatureVerificationOptions>(options =>
        {
            options.TagsToVerify.Add(WebhookSender.TagName);

            options.RequiredSignatureComponents.Add(SignatureComponent.RequestTargetUri);
            options.RequiredSignatureComponents.Add(SignatureComponent.ContentDigest);
            options.RequiredSignatureComponents.Add(SignatureComponent.ContentLength);
            options.RequiredSignatureComponents.Add(new HttpHeaderComponent("ce-id"));
            options.RequiredSignatureComponents.Add(new HttpHeaderComponent("ce-type"));
            options.RequiredSignatureComponents.Add(new HttpHeaderComponent("ce-time"));

            options.CreatedRequired = true;
            options.ExpiresRequired = true;
            options.KeyIdRequired = true;
            options.AlgorithmRequired = true;
            options.TagRequired = true;

            options.MaxSignatureAge = TimeSpan.FromMinutes(5);

            options.VerifyNonce = _ => true;
        });

        builder.Services.AddSignatureVerification(new ECDsaP382Sha384SignatureProvider(certificate, "key"));

        var app = builder.Build();

        app.UseSignatureVerification();

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

    public IWebhookSender GetWebhookSender() => Services.GetRequiredService<IWebhookSender>();

    public WebhookOptions GetWebhookOptions() => Services.GetRequiredService<IOptions<WebhookOptions>>().Value;

    public void Dispose() => _server.Dispose();
}
