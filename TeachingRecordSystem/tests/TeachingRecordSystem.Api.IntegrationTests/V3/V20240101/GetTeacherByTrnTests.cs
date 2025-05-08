using System.Text.Json;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240101;

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

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson])]
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

        await ValidRequestForTeacher_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var dqtStatus = dfeta_InductionStatus.Pass;
        var startDate = new DateOnly(1996, 2, 3);
        var completedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithInductionStatus(i => i.WithStatus(inductionStatus).WithStartDate(startDate).WithCompletedDate(completedDate)));

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");

        AssertEx.JsonObjectEquals(
            new
            {
                startDate = startDate.ToString("yyyy-MM-dd"),
                endDate = completedDate.ToString("yyyy-MM-dd"),
                status = dqtStatus.ToString(),
                statusDescription = dqtStatus.GetDescription(),
                certificateUrl = "/v3/certificates/induction",
                periods = Array.Empty<object>()
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_ValidRequestWithInductionAndPersonHasNullDqtStatus_ReturnsNullInductionContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");
        Assert.Equal(JsonValueKind.Null, responseInduction.ValueKind);
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
        var qualifications = new[]
        {
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.NPQLL, null, IsActive:true),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.NPQSL, new DateOnly(2022, 5, 6), IsActive:false),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.NPQEYL, new DateOnly(2022, 3, 4), IsActive:true)
        };

        var contact = await CreateContact(qualifications: qualifications);
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(GetHttpClientWithApiKey(), baseUrl, contact, qualifications);
    }

    [Fact]
    public async Task Get_ValidRequestWithMandatoryQualifications_ReturnsExpectedMandatoryQualificationsContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
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
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
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
        var qualifications = new[]
        {
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.HigherEducation, new DateOnly(2022, 4, 6), true,  "001", "001", "002", "003"),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.HigherEducation, new DateOnly(2022, 4, 2), true,  "002", "002"),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.HigherEducation, null, true,  "001", "003"),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.HigherEducation, new DateOnly(2022, 4, 8), false,  "001", "001", "002", "003"),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.HigherEducation, null, true,  null, "003"),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.HigherEducation, new DateOnly(2022, 4, 8), true),
        };

        var contact = await CreateContact(qualifications: qualifications);
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(GetHttpClientWithApiKey(), baseUrl, contact, qualifications);
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
        // Arrange
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
        var alertType = alertTypes.Where(at => Api.V3.Constants.LegacyExposableSanctionCodes.Contains(at.DqtSanctionCode)).RandomOne();

        var person = await TestData.CreatePersonAsync(b => b
            .WithTrn()
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

        var alert = person.Alerts.Last();

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{person.Trn}?include=Sanctions");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseSanctions = jsonResponse.RootElement.GetProperty("sanctions");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    code = alert.AlertType!.DqtSanctionCode,
                    startDate = alert.StartDate
                }
            },
            responseSanctions);
    }

    [Fact]
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedAlertsContent()
    {
        // Arrange
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
        var alertType = alertTypes.Where(at => Api.V3.Constants.LegacyProhibitionSanctionCodes.Contains(at.DqtSanctionCode)).RandomOne();

        var person = await TestData.CreatePersonAsync(b => b
            .WithTrn()
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

        var alert = person.Alerts.Last();

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{person.Trn}?include=Alerts");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseAlerts = jsonResponse.RootElement.GetProperty("alerts");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    alertType = "Prohibition",
                    dqtSanctionCode = alert.AlertType!.DqtSanctionCode,
                    startDate = alert.StartDate,
                    endDate = alert.EndDate
                }
            },
            responseAlerts);
    }

    [Fact]
    public async Task Get_ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/teachers/{contact.dfeta_TRN}";

        await ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }
}
