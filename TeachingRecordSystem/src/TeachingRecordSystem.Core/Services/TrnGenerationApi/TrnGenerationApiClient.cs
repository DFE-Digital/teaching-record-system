namespace TeachingRecordSystem.Core.Services.TrnGenerationApi;

public class TrnGenerationApiClient : ITrnGenerationApiClient
{
    private readonly HttpClient _httpClient;

    public TrnGenerationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateTrn()
    {
        var response = await _httpClient.PostAsync("/api/v1/trn-requests", null);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = $"Error calling REST API to generate a TRN, Status Code {response.StatusCode}, Reason {response.ReasonPhrase}.";
            throw new InvalidOperationException(errorMessage);
        }

        var trn = await response.Content.ReadAsStringAsync();
        return trn;
    }
}
