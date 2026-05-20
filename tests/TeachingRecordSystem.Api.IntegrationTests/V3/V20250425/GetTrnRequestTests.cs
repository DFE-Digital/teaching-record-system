using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250425;

public class GetTrnRequestTests : TestBase
{
    public GetTrnRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.CreateTrn]);

        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse()
            {
                Email = req.Email,
                ExpiresUtc = Clock.UtcNow.AddDays(1),
                Trn = req.Trn,
                TrnToken = Guid.NewGuid().ToString()
            });
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
        var aytqLink = await GetAccessYourTeachingQualificationsLinkAsync(requestId);

        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId,
                trn = existingPerson.Trn,
                status = "Completed",
                potentialDuplicate = false,
                accessYourTeachingQualificationsLink = aytqLink
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
                trn = (string?)null,
                status = "Pending",
                potentialDuplicate = true,
                accessYourTeachingQualificationsLink = (string?)null
            },
            expectedStatusCode: 200);
    }

    private async Task<string> GetAccessYourTeachingQualificationsLinkAsync(string requestId)
    {
        var trnToken = await WithDbContextAsync(async dbContext =>
        {
            var metadata = await dbContext.TrnRequestMetadata.SingleAsync(r => r.ApplicationUserId == ApplicationUserId && r.RequestId == requestId);

            if (metadata.TrnToken is null)
            {
                throw new InvalidOperationException("TRN request does not have a TRN token.");
            }

            return metadata.TrnToken!;
        });

        return HostFixture.Services.GetRequiredService<TrnRequestService>()
            .GetAccessYourTeachingQualificationsLink(trnToken);
    }
}
