namespace DqtApi.Services.TrnGenerationApi
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class TrnGenerationApiClient : ITrnGenerationApiClient
    {
        private readonly HttpClient _httpClient;

        public TrnGenerationApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateTrnAsync()
        {
            string nextTrn = null;
            var response = await _httpClient.PostAsync("/api/v1/trn-requests", null);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = $"Error calling REST API to generate a TRN, Status Code {response.StatusCode}, Reason {response.ReasonPhrase}.";
                throw new InvalidOperationException(errorMessage);
            }

            nextTrn = await response.Content.ReadAsStringAsync();
            return nextTrn;
        }
    }
}
