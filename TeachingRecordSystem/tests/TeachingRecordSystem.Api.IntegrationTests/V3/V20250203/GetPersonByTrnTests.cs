using TeachingRecordSystem.Api.V3.Implementation.Dtos;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250203;

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
        var status = InductionStatus.Passed;
        var startDate = new DateOnly(1996, 2, 3);
        var completedDate = new DateOnly(1996, 6, 7);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)));

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
                completedDate = completedDate.ToString("yyyy-MM-dd")
            },
            responseInduction);
    }

    [Fact]
    public async Task Get_WithQtlsDate_ReturnsActiveQtlsStatus()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQtlsDate(qtlsDate));

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.Active.ToString(), qtlsStatus!);
    }

    [Fact]
    public async Task Get_WithExpiredQtlsDate_ReturnsExpiredQtlsStatus()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQtlsDate(qtlsDate));

        var entity = new Microsoft.Xrm.Sdk.Entity() { Id = person.PersonId, LogicalName = Contact.EntityLogicalName };
        entity[Contact.Fields.dfeta_qtlsdate] = null;
        await TestData.OrganizationService.UpdateAsync(entity);

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.Expired.ToString(), qtlsStatus!);
    }

    [Fact]
    public async Task Get_WithoutQtlsDate_ReturnsNoneQtlsStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}?include=Induction");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var jsonResponse = await AssertEx.JsonResponseAsync(response);
        var qtlsStatus = jsonResponse.RootElement.GetProperty("qtlsStatus").GetString();
        Assert.Equal(QtlsStatus.None.ToString(), qtlsStatus!);
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
                completedDate = (DateOnly?)null
            },
            responseInduction);
    }
}
