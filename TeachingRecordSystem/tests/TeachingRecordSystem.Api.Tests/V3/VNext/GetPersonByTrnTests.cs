namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public class GetPersonByTrnTests : TestBase
{
    public GetPersonByTrnTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(roles: [ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_AppropriateBodyUserSpecifiesNationalInsuranceNumber_ReturnsForbidden()
    {
        // Arrange
        SetCurrentApiClient([ApiRoles.AppropriateBody]);

        var person = await TestData.CreatePersonAsync(x => x.WithTrn().WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}?nationalInsuranceNumber={person.NationalInsuranceNumber}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_BothNationalInsuranceNumberAndDateOfBirthSpecified_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithTrn().WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}&nationalInsuranceNumber={person.NationalInsuranceNumber}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithNationalInsuranceNumberMatchingRecord_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithTrn().WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}?nationalInsuranceNumber={person.NationalInsuranceNumber}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseAsync(response, expectedStatusCode: 200);
    }

    [Fact]
    public async Task Get_WithNationalInsuranceNumberNotMatchingRecord_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithTrn().WithNationalInsuranceNumber());
        var requestNino = TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}?nationalInsuranceNumber={requestNino}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithNationalInsuranceNumberMatchingWorkforceData_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());

        var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321");
        var employmentNino = TestData.GenerateNationalInsuranceNumber();
        await TestData.CreateTpsEmploymentAsync(
            person,
            establishment,
            startDate: new DateOnly(2024, 1, 1),
            lastKnownEmployedDate: new DateOnly(2024, 10, 1),
            EmploymentType.FullTime,
            lastExtractDate: new DateOnly(2024, 10, 1),
            nationalInsuranceNumber: employmentNino);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}?nationalInsuranceNumber={employmentNino}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseAsync(response, expectedStatusCode: 200);
    }

    [Fact]
    public async Task Get_WithNonNullDqtInductionStatus_ReturnsExpectedInduction()
    {
        // Arrange
        var dqtStatus = dfeta_InductionStatus.Pass;
        var status = dqtStatus.ToInductionStatus();
        var startDate = new DateOnly(1996, 2, 3);
        var completedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithDqtInduction(
                dqtStatus,
                inductionExemptionReason: null,
                startDate,
                completedDate));

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");

        AssertEx.JsonObjectEquals(
            new
            {
                status = status.ToString(),
                startDate = startDate.ToString("yyyy-MM-dd"),
                completedDate = completedDate.ToString("yyyy-MM-dd"),
                certificateUrl = "/v3/certificates/induction"
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_WithNullDqtInductionStatus_ReturnsNoneInductionStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var responseInduction = jsonResponse.RootElement.GetProperty("induction");

        AssertEx.JsonObjectEquals(
            new
            {
                status = InductionStatus.None.ToString(),
                startDate = (DateOnly?)null,
                completedDate = (DateOnly?)null,
                certificateUrl = (string?)null
            },
            responseInduction);
    }
}
