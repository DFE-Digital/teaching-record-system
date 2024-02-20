using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.Api.Tests.V3.V20240101;

public class GetTeacherByTrnTests : GetTeacherTestBase
{
    public GetTeacherByTrnTests(HostFixture hostFixture)
        : base(hostFixture)
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
        var response = await HostFixture.CreateClient().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson, ApiRoles.UpdatePerson])]
    public async Task GetTeacher_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var trn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

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
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForTeacher_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, expectQtsCertificateUrl: false, expectEysCertificateUrl: false, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
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

        await ValidRequestForTeacher_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, expectQtsCertificateUrl: false, expectEysCertificateUrl: false, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), "Qualified"), expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, expectCertificateUrls: false, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithInduction_ReturnsExpectedInductionContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(GetHttpClientWithApiKey(), baseUrl, contact, expectCertificateUrls: false);
    }

    [Fact]
    public async Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b
            .WithTrn()
            // MQ with no EndDate
            .WithMandatoryQualification(b => b.WithStatus(MandatoryQualificationStatus.InProgress))
            // MQ with no Specialism
            .WithMandatoryQualification(b => b.WithSpecialism(null))
            // MQ with EndDate and Specialism
            .WithMandatoryQualification(b => b
                .WithStatus(MandatoryQualificationStatus.Passed, endDate: new(2022, 9, 1))
                .WithSpecialism(MandatoryQualificationSpecialism.Auditory)));

        var validMq = person.MandatoryQualifications.Last();

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{person.Trn}?include=MandatoryQualifications");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponse(response);
        var responseMandatoryQualifications = jsonResponse.RootElement.GetProperty("mandatoryQualifications");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    awarded = validMq.EndDate?.ToString("yyyy-MM-dd"),
                    specialism = validMq.Specialism?.GetTitle()
                }
            },
            responseMandatoryQualifications);
    }

    [Fact]
    public async Task Get_ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithSanctions_ReturnsExpectedSanctionsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithSanctions_ReturnsExpectedSanctionsContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedSanctionsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithAlerts_ReturnsExpectedSanctionsContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }
}
