using System.Net;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public class GetQTLSDateRequestTests : TestBase
{
    public GetQTLSDateRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.UpdatePerson });
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdatePerson)]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{existingContact.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Trn_TrnNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTrn = "1234567";

        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{nonExistentTrn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task Get_NoQTLS_ReturnsExpectedResult()
    {
        // Arrange  
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{existingContact.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                Trn = existingContact.Trn,
                AwardedDate = default(DateOnly?)

            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Get_WithQTLS_ReturnsExpectedResult()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber)
            .WithQTLSDate(qtlsDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"v3/persons/{existingContact.Trn}/qtls");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                Trn = existingContact.Trn,
                AwardedDate = qtlsDate

            },
            expectedStatusCode: 200);
    }
}
