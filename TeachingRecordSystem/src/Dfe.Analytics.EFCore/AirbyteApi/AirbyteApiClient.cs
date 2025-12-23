using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.Analytics.EFCore.AirbyteApi;

public class AirbyteApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    // public async Task<GetConnectionResponse> GetConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
    // {
    //     var response = await httpClient.GetAsync($"api/v1/connections/{Uri.EscapeDataString(connectionId)}", cancellationToken);
    //     response.EnsureSuccessStatusCode();
    //
    //     return (await response.Content.ReadFromJsonAsync<GetConnectionResponse>(_serializerOptions, cancellationToken))!;
    // }

    // public async Task<GetJobsListResponse> GetJobsListAsync(
    //     string connectionId,
    //     int? limit = null,
    //     string? jobType = null,
    //     string? orderBy = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     var response = await httpClient.GetAsync(
    //         $"api/v1/jobs?connectionId={Uri.EscapeDataString(connectionId)}&" +
    //         $"limit={limit?.ToString() ?? string.Empty}&" +
    //         $"jobType={Uri.EscapeDataString(jobType ?? string.Empty)}&" +
    //         $"orderBy={Uri.EscapeDataString(orderBy ?? string.Empty)}",
    //         cancellationToken);
    //     response.EnsureSuccessStatusCode();
    //
    //     return (await response.Content.ReadFromJsonAsync<GetJobsListResponse>(_serializerOptions, cancellationToken))!;
    // }
    //
    // public async Task<GetJobStatusResponse> GetJobStatusAsync(long jobId, CancellationToken cancellationToken = default)
    // {
    //     var response = await httpClient.GetAsync($"api/v1/jobs/{jobId}", cancellationToken);
    //     response.EnsureSuccessStatusCode();
    //
    //     return (await response.Content.ReadFromJsonAsync<GetJobStatusResponse>(_serializerOptions, cancellationToken))!;
    // }

    public async Task UpdateConnectionDetailsAsync(string connectionId, UpdateConnectionDetailsRequest request, CancellationToken cancellationToken = default)
    {
        var content = CreateJsonContent(request);

        var response = await httpClient.PatchAsync(
            $"api/public/v1/connections/{Uri.EscapeDataString(connectionId)}",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    // public async Task<TriggerJobResponse> TriggerJobAsync(string connectionId, string jobType, CancellationToken cancellationToken = default)
    // {
    //     var requestBody = new
    //     {
    //         connectionId = connectionId,
    //         jobType = jobType
    //     };
    //
    //     var response = await httpClient.PostAsJsonAsync("api/v1/jobs/trigger", requestBody, _serializerOptions, cancellationToken);
    //     response.EnsureSuccessStatusCode();
    //
    //     return (await response.Content.ReadFromJsonAsync<TriggerJobResponse>(_serializerOptions, cancellationToken))!;
    // }

    internal static void ConfigureHttpClient(IHttpClientBuilder clientBuilder)
    {
        ArgumentNullException.ThrowIfNull(clientBuilder);

        clientBuilder
            .ConfigureHttpClient((sp, client) =>
            {
                var optionsAccessor = sp.GetRequiredService<IOptions<AirbyteOptions>>();
                client.BaseAddress = new Uri(optionsAccessor.Value.ApiBaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler(sp => new AuthenticatingHandler(sp.GetRequiredService<IOptions<AirbyteOptions>>()));
    }

    private static HttpContent CreateJsonContent(object value)
    {
        // Airbyte API is fussy about the Content-Type header; it must be exactly "application/json"
        return JsonContent.Create(value, mediaType: new MediaTypeHeaderValue("application/json"), _serializerOptions);
    }

    private class AuthenticatingHandler(IOptions<AirbyteOptions> optionsAccessor) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Access tokens only live for 3 minutes - Airbyte docs recommends getting a new token for each request

            var accessToken = await EnsureAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> EnsureAccessTokenAsync(CancellationToken cancellationToken)
        {
            var options = optionsAccessor.Value;

            var requestBody = new
            {
                client_id = options.ClientId,
                client_secret = options.ClientSecret,
                grant_type = "client_credentials"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, options.ApiBaseUrl.TrimEnd('/') + "/api/public/v1/applications/token")
            {
                Content = JsonContent.Create(requestBody)
            };

            var response = await base.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
            var root = responseJson!.RootElement;

            return root.GetProperty("access_token").GetString()!;
        }
    }
}
