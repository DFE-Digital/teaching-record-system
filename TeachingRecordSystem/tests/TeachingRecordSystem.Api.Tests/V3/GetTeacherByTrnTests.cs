using Xunit;

namespace TeachingRecordSystem.Api.Tests.V3;

public class GetTeacherByTrnTests : GetTeacherTestBase
{
    public GetTeacherByTrnTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
    }

    [Fact]
    public async Task Get_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var trn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{trn}");

        // Act
        var response = await ApiFixture.CreateClient().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TrnNotFound_ReturnsNotFound()
    {
        // Arrange
        var trn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{trn}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestForTeacher_ReturnsExpectedContent(HttpClientWithApiKey, baseUrl, trn, qualifiedInWales: false, expectQtsCertificateUrl: false, expectEysCertificateUrl: false);
    }

    [Fact]
    public Task Get_ValidRequestForTeacherQualifiedInWales_ReturnsExpectedResponse()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestForTeacher_ReturnsExpectedContent(HttpClientWithApiKey, baseUrl, trn, qualifiedInWales: true, expectQtsCertificateUrl: false, expectEysCertificateUrl: false);
    }

    [Fact]
    public Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(HttpClientWithApiKey, baseUrl, trn, expectCertificateUrls: false);
    }

    [Fact]
    public Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestWithInduction_ReturnsExpectedInductionContent(HttpClientWithApiKey, baseUrl, trn, expectCertificateUrls: false);
    }

    [Fact]
    public Task Get_ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent(HttpClientWithApiKey, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(HttpClientWithApiKey, baseUrl, trn, expectCertificateUrls: false);
    }

    [Fact]
    public Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent(HttpClientWithApiKey, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(HttpClientWithApiKey, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(HttpClientWithApiKey, baseUrl, trn);
    }

    [Fact]
    public Task Get_ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue()
    {
        var trn = "1234567";
        var baseUrl = $"/v3/teachers/{trn}";

        return ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(HttpClientWithApiKey, baseUrl, trn);
    }
}
