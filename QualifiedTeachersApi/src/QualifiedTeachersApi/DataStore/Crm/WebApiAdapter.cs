#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace QualifiedTeachersApi.DataStore.Crm;

public class WebApiAdapter : IWebApiAdapter
{
    private static readonly TimeSpan _tokenExpirationBuffer = TimeSpan.FromSeconds(30);

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private (string AccessToken, DateTime Expires)? _currentAccessToken;

    public WebApiAdapter(IConfiguration configuration)
    {
        _configuration = configuration;

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(configuration["CrmUrl"].TrimEnd('/') + "/api/data/v9.2/");
        _httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
        _httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<(int NumberOfRequests, double RemainingExecutionTime)> GetRemainingApiLimits()
    {
        await EnsureAccessToken();

        var whoAmIResponse = await _httpClient.GetAsync("WhoAmI");
        whoAmIResponse.EnsureSuccessStatusCode();

        var numberOfRequests = int.Parse(whoAmIResponse.Headers.NonValidated["x-ms-ratelimit-burst-remaining-xrm-requests"].Single());
        var executionTime = double.Parse(whoAmIResponse.Headers.NonValidated["x-ms-ratelimit-time-remaining-xrm-requests"].Single());

        return (numberOfRequests, executionTime);
    }

    private async Task EnsureAccessToken()
    {
        if (!_currentAccessToken.HasValue ||
            _currentAccessToken.Value.Expires.Subtract(_tokenExpirationBuffer) <= DateTime.UtcNow)
        {
            _currentAccessToken = await GetAccessToken();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken.Value.AccessToken);
        }
    }

    private async Task<(string AccessToken, DateTime Expires)> GetAccessToken()
    {
        var scope = $"https://{(new Uri(_configuration["CrmUrl"]).Host)}/.default";
        var clientId = _configuration["CrmClientId"];
        var clientSecret = _configuration["CrmClientSecret"];

        using var oauthClient = new HttpClient();

        var request = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/fad277c9-c60a-4da1-b5f3-b3b8b34a82f9/oauth2/v2.0/token");

        var postParams = new Dictionary<string, string>()
        {
            { "grant_type", "client_credentials" },
            { "scope", scope },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        request.Content = new FormUrlEncodedContent(postParams);

        var response = await oauthClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        var expires = DateTime.UtcNow.Add(TimeSpan.FromSeconds(tokenResponse.ExpiresIn));

        return (tokenResponse.AccessToken, expires);
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
