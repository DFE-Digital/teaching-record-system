#nullable disable
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

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

    public async Task<GetAnIdentityApiUser> GetUserById(Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/v1/users/{userId}");

        switch ((int)response.StatusCode)
        {
            case StatusCodes.Status200OK:
                break;
            case StatusCodes.Status404NotFound:
                return null;
            default:
                response.EnsureSuccessStatusCode();
                break;
        }

        var user = await response.Content.ReadFromJsonAsync<GetAnIdentityApiUser>();
        return user;
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
        public string Trn { get; set; }
    }
}

