#nullable disable
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
        var baseUrl = "/v3/teacher";

        return ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, trn, expectCertificateUrls: true);
    }

    [Fact]
    public Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(httpClient, baseUrl, trn, expectCertificateUrls: true);
    }

    [Fact]
    public Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestWithInduction_ReturnsExpectedInductionContent(httpClient, baseUrl, trn, expectCertificateUrls: true);
    }

    [Fact]
    public Task Get_ValidRequestWithInitialTeacherTraining_ReturnsExpectedInductionContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestWithInitialTeacherTraining_ReturnsExpectedInductionContent(httpClient, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestWithNpqQualifications_ReturnsExpectedInductionContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestWithNpqQualifications_ReturnsExpectedInductionContent(httpClient, baseUrl, trn, expectCertificateUrls: true);
    }

    [Fact]
    public Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedInductionContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestWithMandatoryQualifications_ReturnsExpectedInductionContent(httpClient, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "v3/teacher";

        return ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(httpClient, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "v3/teacher";

        return ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(httpClient, baseUrl, trn);
    }
}
