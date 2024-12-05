using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.Api.Tests.V3.V20240101;

public class GetTeacherTests : GetTeacherTestBase
{
    public GetTeacherTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_TeacherWithTrnDoesNotExist_ReturnsForbidden()
    {
        // Arrange
        var trn = "1234567";
        var httpClient = GetHttpClientWithIdentityAccessToken(trn);

        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/teacher");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("28", "Qualified")]
    [InlineData("50", "Qualified")]
    [InlineData("67", "Qualified")]
    [InlineData("68", "Qualified")]
    [InlineData("69", "Qualified")]
    [InlineData("71", "Qualified")]
    [InlineData("87", "Qualified")]
    [InlineData("90", "Qualified")]
    [InlineData("100", "Qualified")]
    [InlineData("103", "Qualified")]
    [InlineData("104", "Qualified")]
    [InlineData("206", "Qualified")]
    [InlineData("211", "Trainee teacher")]
    [InlineData("212", "Assessment only route candidate")]
    [InlineData("214", "Partial qualified teacher status")]
    public async Task Get_ValidRequestWithSingleQts_ReturnsExpectedResponse(string qtsStatusValue, string qtsStatusDescription)
    {
        var qtsDate = new DateOnly(2021, 01, 01);
        var qtsCreatedDate = new DateTime(2021, 01, 01);
        var qts = new QtsRegistration[]
        {
            new QtsRegistration(qtsDate, qtsStatusValue, qtsCreatedDate, null, null)
        };
        var contact = await CreateContact(qts);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";
        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), qtsStatusDescription), expectedEyts: null);
    }

    [Theory]
    [InlineData("220", "Early years trainee")]
    [InlineData("221", "Qualified")]
    [InlineData("222", "Early years professional status")]
    public async Task Get_ValidRequestWithSingleEYTS_ReturnsExpectedResponse(string eytsStatusValue, string eytsStatusDescription)
    {
        var eytsDate = new DateOnly(2021, 01, 01);
        var createdDate = new DateTime(2021, 01, 01);
        var qts = new QtsRegistration[]
        {
            new QtsRegistration(null, null, createdDate, eytsDate, eytsStatusValue)
        };
        var contact = await CreateContact(qts);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";
        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, qtsRegistrations: qts, expectedQts: null, expectedEyts: (eytsDate.ToDateTime(), eytsStatusDescription));
    }

    [Fact]
    public async Task Get_ValidRequestWithoutEYTSorQTS_ReturnsExpectedResponse()
    {
        var contact = await CreateContact(null);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";
        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestWithEYTSandQTS_ReturnsExpectedResponse()
    {
        var qtsDate = new DateOnly(2021, 05, 15);
        var eytsDate = new DateOnly(2021, 01, 01);
        var createdDate = new DateTime(2021, 01, 01);
        var qts = new QtsRegistration[]
        {
            new QtsRegistration(qtsDate, "212", createdDate, eytsDate, "220")
        };
        var contact = await CreateContact(qts);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), "Assessment only route candidate"), expectedEyts: (eytsDate.ToDateTime(), "Early years trainee"));
    }

    [Fact]
    public async Task Get_MultipleQTSRecords_ReturnsMostRecent()
    {
        var qtsDate1 = new DateOnly(2021, 05, 15);
        var qtsDate2 = new DateOnly(2021, 06, 15);
        var createdDate1 = new DateTime(2021, 05, 15);
        var createdDate2 = new DateTime(2021, 06, 15);
        var qts = new QtsRegistration[]
        {
            new QtsRegistration(qtsDate1, "212", createdDate1, null, null),
            new QtsRegistration(qtsDate2, "212", createdDate2, null, null)
        };
        var contact = await CreateContact(qts);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, qtsRegistrations: qts, expectedQts: (qtsDate2.ToDateTime(), "Assessment only route candidate"), expectedEyts: null);
    }

    [Fact]
    public async Task Get_MultipleEYTSRecords_ReturnsMostRecent()
    {
        var eytsDate1 = new DateOnly(2021, 05, 15);
        var eytsDate2 = new DateOnly(2021, 06, 15);
        var createdDate1 = new DateTime(2021, 05, 15);
        var createdDate2 = new DateTime(2021, 06, 15);
        var qts = new QtsRegistration[]
        {
            new QtsRegistration(null, null, createdDate1, eytsDate1, "220"),
            new QtsRegistration(null, null, createdDate2, eytsDate2, "220")
        };
        var contact = await CreateContact(qts);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, qtsRegistrations: qts, expectedQts: null, expectedEyts: (eytsDate2.ToDateTime(), "Early years trainee"));
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
    {
        var qtsDate = new DateOnly(2021, 05, 15);
        var eytsDate = new DateOnly(2021, 01, 01);
        var createdDate = new DateTime(2021, 01, 01);
        var qts = new QtsRegistration[]
        {
            new QtsRegistration(qtsDate, "212", createdDate, eytsDate, "220")
        };
        var contact = await CreateContact(hasMultiWordFirstName: true, qtsRegistrations: qts);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(httpClient, baseUrl, contact, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), "Assessment only route candidate"), expectedEyts: (eytsDate.ToDateTime(), "Early years trainee"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        // Arrange
        var dqtStatus = dfeta_InductionStatus.Pass;
        var startDate = new DateOnly(1996, 2, 3);
        var completedDate = new DateOnly(1996, 6, 7);
        var abName = "Test AB";
        var appropriateBody = await TestData.CreateAccountAsync(a => a.WithName(abName));
        var numberOfTerms = 3;

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithDqtInduction(
                dqtStatus,
                inductionExemptionReason: null,
                startDate,
                completedDate,
                inductionPeriodStartDate: startDate,
                inductionPeriodEndDate: completedDate,
                appropriateBodyOrgId: appropriateBody.Id,
                numberOfTerms: numberOfTerms));

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/teacher?include=Induction");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

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
                periods = new[]
                {
                    new
                    {
                        startDate = startDate.ToString("yyyy-MM-dd"),
                        endDate = completedDate.ToString("yyyy-MM-dd"),
                        terms = numberOfTerms,
                        appropriateBody = new
                        {
                            name = abName
                        }
                    }
                }
            },
            responseInduction);
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
        var qualifications = new[]
{
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.NPQLL, null, IsActive:true),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.NPQSL, new DateOnly(2022, 5, 6), IsActive:false),
            new Qualification(Guid.NewGuid(), dfeta_qualification_dfeta_Type.NPQEYL, new DateOnly(2022, 3, 4), IsActive:true)
        };

        var contact = await CreateContact(qualifications: qualifications);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(httpClient, baseUrl, contact, qualifications);
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
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/teacher?include=MandatoryQualifications");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

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
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(httpClient, baseUrl, contact, qualifications);
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
        // Arrange
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypesAsync();
        var alertType = alertTypes.Where(at => Api.V3.Constants.LegacyExposableSanctionCodes.Contains(at.DqtSanctionCode)).RandomOne();

        var person = await TestData.CreatePersonAsync(b => b
            .WithTrn()
            .WithAlert(a => a.WithAlertTypeId(alertType.AlertTypeId).WithEndDate(null)));

        var alert = person.Alerts.Last();

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teacher?include=Sanctions");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseSanctions = jsonResponse.RootElement.GetProperty("sanctions");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    code = alert.AlertType.DqtSanctionCode,
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teacher?include=Alerts");

        // Act
        var response = await GetHttpClientWithIdentityAccessToken(person.Trn!).SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseAlerts = jsonResponse.RootElement.GetProperty("alerts");

        AssertEx.JsonObjectEquals(
            new[]
            {
                new
                {
                    alertType = "Prohibition",
                    dqtSanctionCode = alert.AlertType.DqtSanctionCode,
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
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "v3/teacher";

        await ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent(httpClient, baseUrl, contact);
    }
}
