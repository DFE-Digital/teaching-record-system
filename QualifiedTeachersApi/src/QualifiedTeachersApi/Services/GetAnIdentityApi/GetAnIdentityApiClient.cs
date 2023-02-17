using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QualifiedTeachersApi.Json;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

public class GetAnIdentityApiClient : IGetAnIdentityApiClient
{
    private readonly HttpClient _httpClient;
    private System.Text.Json.JsonSerializerOptions _jsonOptions { get; set; }

    public GetAnIdentityApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new System.Text.Json.JsonSerializerOptions().AddConverters();
    }

    public async Task<GetAnIdentityApiUser> GetUserById(Guid userId)
    {
        var response = await _httpClient.GetAsync($"api/v1/users/{userId}");
        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<GetAnIdentityApiUser>();
            return user;
        }
        return null;
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

