using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20240307;

public class GetTrnRequestTests : TestBase
{
    public GetTrnRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.CreateTrn]);
    }

    [Theory, RoleNamesData(except: ApiRoles.CreateTrn)]
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

        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = existingPerson.PersonId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestId={requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_TrnRequestNotFound_ReturnsNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestId={requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidCompletedTrnRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber)
            .WithTrnRequest(ApplicationUserId, requestId));

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestId={requestId}");

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName,
                    middleName,
                    lastName,
                    email,
                    dateOfBirth,
                    nationalInsuranceNumber
                },
                trn = existingPerson.Trn,
                status = "Completed"
            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Get_ValidPendingTrnRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        await TestData.CreateApiTrnRequestSupportTaskAsync(ApplicationUserId, t => t
            .WithRequestId(requestId)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber));

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v3/trn-requests?requestId={requestId}");

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId,
                person = new
                {
                    firstName,
                    middleName,
                    lastName,
                    email,
                    dateOfBirth,
                    nationalInsuranceNumber
                },
                trn = (string?)null,
                status = "Pending"
            },
            expectedStatusCode: 200);
    }
}
