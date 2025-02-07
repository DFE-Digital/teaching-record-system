using System.Net.Http.Json;
using Polly;

namespace TeachingRecordSystem.Core.Services.PublishApi;

public class PublishApiClient : IPublishApiClient
{
    private readonly string[] noLongerAccreditedTrainingProviders = ["10034865", "10005413", "10002327", "10046628", "10055126", "10064183", "10007938"];
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline _resiliencePipeline;

    public PublishApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new Polly.Retry.RetryStrategyOptions()
            {
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromSeconds(0),
                MaxRetryAttempts = 2
            })
            .Build();
    }

    public async Task<IReadOnlyCollection<ProviderResource>> GetAccreditedProvidersAsync()
    {
        var response = await _resiliencePipeline.ExecuteAsync(async _ => await _httpClient.GetAsync("recruitment_cycles/current/providers?filter[is_accredited_body]=true&page[per_page]=500"));

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = $"Error calling Publish API to get a list of accredited training providers, Status Code {response.StatusCode}, Reason {response.ReasonPhrase}.";
            throw new InvalidOperationException(errorMessage);
        }

        var providerList = await response.Content.ReadFromJsonAsync<ProviderListResponse>();
        // Unfortunately the Publish API still includes some training providers that are no longer accredited, so we need to filter these out
        return providerList!.Data.Where(p => p.Attributes.Ukprn is not null && !noLongerAccreditedTrainingProviders.Contains(p.Attributes.Ukprn)).AsReadOnly();
    }
}
