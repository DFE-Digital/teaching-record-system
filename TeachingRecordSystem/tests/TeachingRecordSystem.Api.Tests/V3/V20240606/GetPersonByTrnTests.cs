using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.Api.Tests.V3.V20240606;

public class GetPersonByTrnTests : GetPersonTestBase
{
    public GetPersonByTrnTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_UnauthenticatedRequest_ReturnsUnauthorized()
    {
        // Arrange
        var trn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{trn}");

        // Act
        var response = await GetHttpClient().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Theory, RoleNamesData(except: [ApiRoles.GetPerson])]
    public async Task GetTeacher_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var trn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{trn}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestForTeacher_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithMultiWordFirstName_ReturnsExpectedResponse()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestForTeacherWithMultiWordFirstName_ReturnsExpectedContent(GetHttpClientWithApiKey(), baseUrl, contact, qtsRegistrations: null, expectedQts: null, expectedEyts: null);
    }

    [Fact]
    public async Task Get_ValidRequestWithInduction_ReturnsExpectedInductionContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestWithInduction_ReturnsExpectedInductionContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithInitialTeacherTraining_ReturnsExpectedInitialTeacherTrainingContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

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
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestWithNpqQualifications_ReturnsExpectedNpqQualificationsContent(GetHttpClientWithApiKey(), baseUrl, contact, qualifications);
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=MandatoryQualifications");

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
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestWithHigherEducationQualifications_ReturnsExpectedHigherEducationQualificationsContent(GetHttpClientWithApiKey(), baseUrl, contact, qualifications);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestForContactWithPendingNameChange_ReturnsPendingNameChangeTrue(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestForContactWithPendingDateOfBirthChange_ReturnsPendingDateOfBirthChangeTrue(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithSanctions_ReturnsExpectedSanctionsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestWithSanctions_ReturnsExpectedSanctionsContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithAlerts_ReturnsExpectedSanctionsContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestWithAlerts_ReturnsExpectedSanctionsContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent()
    {
        var contact = await CreateContact();
        var baseUrl = $"/v3/persons/{contact.dfeta_TRN}";

        await ValidRequestWithPreviousNames_ReturnsExpectedPreviousNamesContent(GetHttpClientWithApiKey(), baseUrl, contact);
    }

    [Fact]
    public async Task Get_DateOfBirthDoesNotMatchTeachingRecord_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithTrn());
        var dateOfBirth = person.DateOfBirth.AddDays(1);

        var httpClient = GetHttpClientWithApiKey();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DateOfBirthMatchesTeachingRecord_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithTrn());

        var httpClient = GetHttpClientWithApiKey();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DateOfBirthNotProvided_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithTrn());

        var httpClient = GetHttpClientWithApiKey();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }
}
