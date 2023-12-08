using TeachingRecordSystem.Api.Tests.Attributes;
using static TeachingRecordSystem.TestCommon.CrmTestData;

namespace TeachingRecordSystem.Api.Tests.V3;

public class GetTeacherByTrnTests : GetTeacherTestBase
{
    public GetTeacherByTrnTests(ApiFixture apiFixture)
        : base(apiFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.GetPerson });
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

    [Theory, RoleNamesData(new[] { ApiRoles.GetPerson, ApiRoles.UpdatePerson })]
    public async Task GetTeacher_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var trn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{trn}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
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
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForTeacher_ReturnsExpectedContent(HttpClientWithApiKey, baseUrl, contact, expectQtsCertificateUrl: false, expectEysCertificateUrl: false, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestForTeacherQualifiedInWales_ReturnsExpectedResponse()
    {
        var qtsDate = new DateOnly(2021, 01, 01);
        var qtsCreatedDate = new DateTime(2021, 01, 01);
        var qts = new QtsRegistration[]
        {
            new QtsRegistration(qtsDate, QtsAwardedInWalesTeacherStatusValue, qtsCreatedDate, null, null)
        };
        var contact = await CreateContact(qts);
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForTeacher_ReturnsExpectedContent(HttpClientWithApiKey, baseUrl, contact, expectQtsCertificateUrl: false, expectEysCertificateUrl: false, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), "Qualified"), expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(HttpClientWithApiKey, baseUrl, contact, expectCertificateUrls: false, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithInduction_ReturnsExpectedInductionContent(HttpClientWithApiKey, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent(HttpClientWithApiKey, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(HttpClientWithApiKey, baseUrl, contact, expectCertificateUrls: false);
    }

    [Fact]
    public async Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent(HttpClientWithApiKey, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(HttpClientWithApiKey, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(HttpClientWithApiKey, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(HttpClientWithApiKey, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithSanctions_ReturnsExpectedSanctionsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithSanctions_ReturnsExpectedSanctionsContent(HttpClientWithApiKey, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedSanctionsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithAlerts_ReturnsExpectedSanctionsContent(HttpClientWithApiKey, baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent(HttpClientWithApiKey, baseUrl, contact);
    }
}
