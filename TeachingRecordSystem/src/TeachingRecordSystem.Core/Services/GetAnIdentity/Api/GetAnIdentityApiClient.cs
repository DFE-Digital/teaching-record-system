using System.Net.Http.Json;
using System.Text.Json;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

namespace TeachingRecordSystem.Core.Services.GetAnIdentityApi;

public class GetAnIdentityApiClient : IGetAnIdentityApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public GetAnIdentityApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<User?> GetUserById(Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/v1/users/{userId}");

        switch ((int)response.StatusCode)
        {
            case 200:
                break;
            case 404:
                return null;
            default:
                response.EnsureSuccessStatusCode();
                break;
        }

        var user = await response.Content.ReadFromJsonAsync<User>();
        return user;
    }

    public async Task<CreateTrnTokenResponse> CreateTrnToken(CreateTrnTokenRequest request)
    {
        HttpContent content = JsonContent.Create(request);
        var response = await _httpClient.PostAsync("/api/v1/trn-tokens", content);
        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync<CreateTrnTokenResponse>();
        return tokenResponse!;
    }

    public async Task SetTeacherTrn(Guid userId, string trn)
    {
        var request = new SetTeacherTrnRequestBody { Trn = trn };
        var content = JsonContent.Create(request, options: _jsonOptions);
        var response = await _httpClient.PutAsync($"api/v1/users/{userId}/trn", content);
        response.EnsureSuccessStatusCode();
    }

    private class SetTeacherTrnRequestBody
    {
        public required string Trn { get; set; }
    }
}

