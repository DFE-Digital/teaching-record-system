using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace QualifiedTeachersApi.Tests.V3;

public class GetTeacherTests : GetTeacherTestBase
{
    public GetTeacherTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task Get_TeacherWithTrnDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/teacher");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var request = new HttpRequestMessage(HttpMethod.Get, "v3/teacher");

        return ValidRequestForTeacher_ReturnsExpectedContent(httpClient, request, trn);
    }

    [Fact]
    public Task Get_ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var request = new HttpRequestMessage(HttpMethod.Get, "v3/teacher");

        return ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(httpClient, request, trn);
    }

    [Fact]
    public Task Get_ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var request = new HttpRequestMessage(HttpMethod.Get, "v3/teacher");

        return ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(httpClient, request, trn);
    }
}
