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
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, qualifiedInWales: false, expectQtsCertificateUrl: true, expectEysCertificateUrl: true);
    }

    [Fact]
    public async Task Get_ValidRequestForTeacherQualifiedInWales_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, qualifiedInWales: true, expectQtsCertificateUrl: false, expectEysCertificateUrl: true);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
    {
        var contact = await CreateContact(hasMultiWordFirstName: true);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(httpClient, baseUrl, contact, expectCertificateUrls: true);
    }

    [Fact]
    public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestWithInduction_ReturnsExpectedInductionContent(httpClient, baseUrl, contact, expectCertificateUrls: true);
    }

    [Fact]
    public async Task Get_ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent(httpClient, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(httpClient, baseUrl, contact, expectCertificateUrls: true);
    }

    [Fact]
    public async Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent(httpClient, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(httpClient, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "v3/teacher";

        await ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(httpClient, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "v3/teacher";

        await ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(httpClient, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithSanctions_ReturnsExpectedSanctionsContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "v3/teacher";

        await ValidRequestWithSanctions_ReturnsExpectedSanctionsContent(httpClient, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedSanctionsContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "v3/teacher";

        await ValidRequestWithAlerts_ReturnsExpectedSanctionsContent(httpClient, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "v3/teacher";

        await ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent(httpClient, baseUrl, contact);
    }
}
