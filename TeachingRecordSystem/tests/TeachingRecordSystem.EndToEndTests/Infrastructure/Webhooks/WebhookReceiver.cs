using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSign.AspNetCore;
using NSign.Providers;
using NSign.Signatures;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.EndToEndTests.Infrastructure.Webhooks;

public sealed class WebhookReceiver : IDisposable
{
    public const string Endpoint = "/webhook";

    private readonly IHost _host;

    public WebhookReceiver()
    {
        var builder = WebApplication.CreateBuilder();

        SigningKey = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var certRequest = new CertificateRequest("CN=Tests", SigningKey, HashAlgorithmName.SHA384);
        Certificate = certRequest.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));

        builder.Services.AddSingleton<WebhookMessageRecorder>();

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

        builder.Services.AddSignatureVerification(new ECDsaP382Sha384SignatureProvider(Certificate, KeyId));

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

        FullyQualifiedEndpoint = app.Urls.First() + Endpoint;

        _host = app;
    }

    public X509Certificate2 Certificate { get; }

    public ECDsa SigningKey { get; }

    public string KeyId { get; } = "key";

    public string FullyQualifiedEndpoint { get; }

    public WebhookMessageRecorder WebhookMessageRecorder => _host.Services.GetRequiredService<WebhookMessageRecorder>();

    public void Dispose() => _host.Dispose();
}
