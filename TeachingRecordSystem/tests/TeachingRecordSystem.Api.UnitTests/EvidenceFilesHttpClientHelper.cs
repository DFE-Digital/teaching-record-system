using JustEat.HttpClientInterception;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Api.UnitTests;

public static class EvidenceFilesHttpClientHelper
{
    public static string EvidenceFileUrl { get; } = Faker.Internet.SecureUrl();
    public static byte[] EvidenceFileContent { get; } = "Test file"u8.ToArray();

    public static void ConfigureServices(IServiceCollection services)
    {
        var options = new HttpClientInterceptorOptions();
        var builder = new HttpRequestInterceptionBuilder();

        var evidenceFileUri = new Uri(EvidenceFileUrl);

        builder
            .Requests()
            .ForGet()
            .ForHttps()
            .ForHost(evidenceFileUri.Host)
            .ForPath(evidenceFileUri.LocalPath.TrimStart('/'))
            .Responds()
            .WithContentStream(() => new MemoryStream(EvidenceFileContent))
            .RegisterWith(options);

        services
            .AddHttpClient("EvidenceFiles")
            .AddHttpMessageHandler(_ => options.CreateHttpMessageHandler())
            .ConfigurePrimaryHttpMessageHandler(_ => new NotFoundHandler());
    }

    private class NotFoundHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}
