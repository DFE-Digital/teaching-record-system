using Xunit;

namespace TeachingRecordSystem.Api.Tests.V3;

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

        return ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, trn, qualifiedInWales: false, expectQtsCertificateUrl: true, expectEysCertificateUrl: true);
    }

    [Fact]
    public Task Get_ValidRequestForTeacherQualifiedInWales_ReturnsExpectedResponse()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, trn, qualifiedInWales: true, expectQtsCertificateUrl: false, expectEysCertificateUrl: true);
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
    public Task Get_ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent(httpClient, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(httpClient, baseUrl, trn, expectCertificateUrls: true);
    }

    [Fact]
    public Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent(httpClient, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "/v3/teacher";

        return ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(httpClient, baseUrl, trn);
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

    [Fact]
    public Task Get_ValidRequestWithSanctions_ReturnsExpectedSanctionsContent()
    {
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);
        var baseUrl = "v3/teacher";

        return ValidRequestWithSanctions_ReturnsExpectedSanctionsContent(httpClient, baseUrl, trn);
    }
}
