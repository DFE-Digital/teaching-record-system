using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20260416;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class GetTrnRequestTests : TestBase
{
    public GetTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
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

    [Fact]
    public async Task Get_UnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var httpClient = GetHttpClient(Version);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/trn-request");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_UserWithoutTrnRequestIdClaim_ReturnsBadRequest()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var httpClient = GetHttpClientWithAuthorizeAccessToken(person.Trn!);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/trn-request");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TrnRequestDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/trn-request");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PendingTrnRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, t => t
            .WithRequestId(trnRequestId)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber));

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/trn-request");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = trnRequestId,
                status = "Pending",
                trn = (string?)null,
                potentialDuplicate = true,
                accessYourTeachingQualificationsLink = (string?)null
            });
    }

    [Fact]
    public async Task Get_CompletedTrnRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber)
            .WithTrnRequest(applicationUser.UserId, trnRequestId));

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequestId);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/trn-request");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        var aytqLink = await GetAccessYourTeachingQualificationsLinkAsync(applicationUser.UserId, trnRequestId);

        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = trnRequestId,
                status = "Completed",
                trn = person.Trn,
                potentialDuplicate = false,
                accessYourTeachingQualificationsLink = aytqLink
            });
    }

    [Fact]
    public async Task Get_DormantTrnRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await TestData.CreateDormantTrnRequestAsync(applicationUser.UserId);

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequest.RequestId);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/trn-request");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = trnRequest.RequestId,
                status = "Dormant",
                trn = (string?)null,
                potentialDuplicate = false,
                accessYourTeachingQualificationsLink = (string?)null
            });
    }

    [Fact]
    public async Task Get_RejectedTrnRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await TestData.CreateRejectedTrnRequestAsync(applicationUser.UserId);

        var httpClient = GetHttpClientWithAuthorizeAccessTokenForTrnRequest(applicationUser.UserId, trnRequest.RequestId);
        var request = new HttpRequestMessage(HttpMethod.Get, "/v3/trn-request");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = trnRequest.RequestId,
                status = "Rejected",
                trn = (string?)null,
                potentialDuplicate = false,
                accessYourTeachingQualificationsLink = (string?)null
            });
    }

    private async Task<string> GetAccessYourTeachingQualificationsLinkAsync(Guid applicationUserId, string requestId)
    {
        var trnToken = await WithDbContextAsync(async dbContext =>
        {
            var metadata = await dbContext.TrnRequestMetadata.SingleAsync(r => r.ApplicationUserId == applicationUserId && r.RequestId == requestId);

            if (metadata.TrnToken is null)
            {
                throw new InvalidOperationException("TRN request does not have a TRN token.");
            }

            return metadata.TrnToken!;
        });

        return HostFixture.Services.GetRequiredService<TrnRequestService>().GetAccessYourTeachingQualificationsLink(trnToken);
    }
}
