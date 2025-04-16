using Polly;

namespace TeachingRecordSystem.Core.Services.TrnGeneration;

public class ApiTrnGenerator : ITrnGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline _resiliencePipeline;

    public ApiTrnGenerator(HttpClient httpClient)
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

    public async Task<string> GenerateTrnAsync()
    {
        var response = await _resiliencePipeline.ExecuteAsync(async _ => await _httpClient.PostAsync("/api/v1/trn-requests", null));

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = $"Error calling REST API to generate a TRN, Status Code {response.StatusCode}, Reason {response.ReasonPhrase}.";
            throw new InvalidOperationException(errorMessage);
        }

        var trn = await response.Content.ReadAsStringAsync();
        return trn;
    }
}
