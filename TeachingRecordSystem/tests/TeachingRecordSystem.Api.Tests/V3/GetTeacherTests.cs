using static TeachingRecordSystem.TestCommon.CrmTestData;

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
        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, expectQtsCertificateUrl: true, expectEysCertificateUrl: false, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), qtsStatusDescription), expectedEyts: null);
    }

    [Theory]
    [InlineData("213", "Qualified")]
    public async Task Get_ValidRequestForTeacherQualifiedInWales_ReturnsExpectedResponse(string qtsStatusValue, string qtsStatusDescription)
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
        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, expectQtsCertificateUrl: false, expectEysCertificateUrl: false, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), qtsStatusDescription), expectedEyts: null);
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
        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, expectQtsCertificateUrl: false, expectEysCertificateUrl: true, qtsRegistrations: qts, expectedQts: null, expectedEyts: (eytsDate.ToDateTime(), eytsStatusDescription));
    }

    [Fact]
    public async Task Get_ValidRequestWithoutEYTSorQTS_ReturnsExpectedResponse()
    {
        var contact = await CreateContact(null);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";
        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, expectQtsCertificateUrl: false, expectEysCertificateUrl: false, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
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

        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, expectQtsCertificateUrl: true, expectEysCertificateUrl: true, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), "Assessment only route candidate"), expectedEyts: (eytsDate.ToDateTime(), "Early years trainee"));
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

        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, expectQtsCertificateUrl: true, expectEysCertificateUrl: false, qtsRegistrations: qts, expectedQts: (qtsDate2.ToDateTime(), "Assessment only route candidate"), expectedEyts: null);
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

        await ValidRequestForTeacher_ReturnsExpectedContent(httpClient, baseUrl, contact, expectQtsCertificateUrl: false, expectEysCertificateUrl: true, qtsRegistrations: qts, expectedQts: null, expectedEyts: (eytsDate2.ToDateTime(), "Early years trainee"));
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
        var contact = await CreateContact(hasMultiWordFirstName: true, qtsQualifications: qts);
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(httpClient, baseUrl, contact, expectCertificateUrls: true, qtsRegistrations: qts, expectedQts: (qtsDate.ToDateTime(), "Assessment only route candidate"), expectedEyts: (eytsDate.ToDateTime(), "Early years trainee"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        var contact = await CreateContact();
        var httpClient = GetHttpClientWithIdentityAccessToken(contact.dfeta_TRN);
        var baseUrl = "/v3/teacher";

        await ValidRequestWithInduction_ReturnsExpectedInductionContent(httpClient, baseUrl, contact, true);
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
