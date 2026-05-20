using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSign;
using NSign.Client;
using NSign.Http;
using NSign.Providers;
using NSign.Signatures;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class WebhookSender(HttpClient httpClient, IOptions<WebhookOptions> optionsAccessor) : IWebhookSender
{
    public const string TagName = "trs-webhooks";
    private const string DataContentType = "application/json; charset=utf-8";
    private const string SignatureName = "sig1";
    private const string UserAgent = "TeachingRecordSystem";
    private const int TimeoutSeconds = 30;

    private readonly CloudEventFormatter _formatter = new JsonEventFormatter();

    public async Task SendMessageAsync(WebhookMessage message, CancellationToken cancellationToken = default)
    {
        Debug.Assert(message.WebhookEndpoint is not null);

        var openApiDocumentName = OpenApiDocumentHelper.GetDocumentName(3, message.ApiVersion);
        var swaggerEndpoint = OpenApiDocumentHelper.DocumentRouteTemplate.Replace("{documentName}", openApiDocumentName);
        var dataSchema = new Uri(optionsAccessor.Value.CanonicalDomain + swaggerEndpoint, UriKind.Absolute);

        var source = new Uri(optionsAccessor.Value.CanonicalDomain);

        var cloudEvent = new CloudEvent
        {
            Id = message.CloudEventId,
            Source = source,
            Type = message.CloudEventType,
            DataContentType = DataContentType,
            DataSchema = dataSchema,
            Time = message.Timestamp,
            Data = message.Data
        };

        var request = new HttpRequestMessage(HttpMethod.Post, message.WebhookEndpoint.Address)
        {
            Content = cloudEvent.ToHttpContent(ContentMode.Binary, _formatter),
            Version = HttpVersion.Version11
        };

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new WebhookMessageDeliveryException(
                response,
                optionsAccessor.Value.CaptureFailedRequests ? await GetRawRequestMessageAsync(request) : null);
        }
    }

    public static void Register(IServiceCollection services, Func<HttpMessageHandler>? getPrimaryHandler = null)
    {
        // We configure the options here manually rather than using the library-provided extension methods so that they don't 'bleed out' globally;
        // it's feasible we could want a different configuration of, say, AddContentDigestOptions for use elsewhere.

        IOptions<AddContentDigestOptions> GetAddContentDigestOptions(IServiceProvider serviceProvider) =>
            Options.Create(new AddContentDigestOptions().WithHash(AddContentDigestOptions.Hash.Sha256));

        IOptions<MessageSigningOptions> GetMessageSigningOptions(IServiceProvider serviceProvider)
        {
            var webhookOptions = serviceProvider.GetRequiredService<IOptions<WebhookOptions>>().Value;
            var keyId = webhookOptions.SigningKeyId;

            var options = new MessageSigningOptions();

            options.SignatureName = SignatureName;

            options
                .WithMandatoryComponent(SignatureComponent.RequestTargetUri)
                .WithMandatoryComponent(SignatureComponent.ContentDigest)
                .WithMandatoryComponent(SignatureComponent.ContentLength)
                .WithMandatoryComponent(new HttpHeaderComponent("ce-id"))
                .WithMandatoryComponent(new HttpHeaderComponent("ce-type"))
                .WithMandatoryComponent(new HttpHeaderComponent("ce-time"))
                .SetParameters = signingOptions => signingOptions
                    .WithTag(TagName)
                    .WithCreatedNow()
                    .WithExpires(DateTimeOffset.UtcNow.AddSeconds(webhookOptions.MessageExpirySeconds))
                    .WithAlgorithm(SignatureAlgorithm.EcdsaP384Sha384)
                    .WithKeyId(keyId)
                    .WithNonce(Guid.NewGuid().ToString("N"));

            return Options.Create(options);
        }

        // The cert is added to the container rather than being created directly in the registration for ISigner
        // so it's tracked by the container and gets disposed with the container.
        services.AddKeyedSingleton(nameof(WebhookSender), (sp, _) =>
        {
            var options = sp.GetRequiredService<IOptions<WebhookOptions>>().Value;
            var signingKeyId = options.SigningKeyId;
            var key = options.Keys.SingleOrDefault(k => k.KeyId == signingKeyId) ??
                throw new Exception($"Key with ID '{signingKeyId}' was not found.");

            return X509Certificate2.CreateFromPem(key.CertificatePem, key.PrivateKeyPem);
        });

        services.AddKeyedSingleton<ISigner>(nameof(WebhookSender), (sp, k) =>
        {
            var options = sp.GetRequiredService<IOptions<WebhookOptions>>().Value;
            var signingKeyId = options.SigningKeyId;
            var cert = sp.GetRequiredKeyedService<X509Certificate2>(k);
            return new ECDsaP382Sha384SignatureProvider(cert, signingKeyId);
        });

        services.AddSingleton<IWebhookSender, WebhookSender>();

        var httpClientBuilder = services
            .AddHttpClient<IWebhookSender, WebhookSender>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
            })
            .AddHttpMessageHandler(sp =>
                ActivatorUtilities.CreateInstance<AddContentDigestHandler>(
                    sp,
                    GetAddContentDigestOptions(sp)))
            .AddHttpMessageHandler(sp =>
                ActivatorUtilities.CreateInstance<SigningHandler>(
                    sp,
                    ActivatorUtilities.CreateInstance<DefaultMessageSigner>(sp, sp.GetRequiredKeyedService<ISigner>(nameof(WebhookSender))),
                    Options.Create(new HttpFieldOptions()),
                    GetMessageSigningOptions(sp)));

        if (getPrimaryHandler is not null)
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => getPrimaryHandler());
        }
    }

    private async Task<string> GetRawRequestMessageAsync(HttpRequestMessage request)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{request.Method} {request.RequestUri} HTTP/{request.Version}");

        foreach (var header in request.Headers)
        {
            sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        if (request.Content is not null)
        {
            foreach (var header in request.Content.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            sb.AppendLine();
            sb.AppendLine(await request.Content.ReadAsStringAsync());
        }
        else
        {
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
