#nullable disable
using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.IntegrationTests.V2.Operations;

public class GetTrnRequestTests : TestBase
{
    public GetTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    [Theory, RoleNamesData(except: [ApiRoles.UpdatePerson])]
    public async Task Get_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var requestId = Guid.NewGuid().ToString();

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TrnRequestDoesNotExistInDbOrCrm_ReturnsNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_TrnRequestDoesNotExistForCurrentClient_ReturnsNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();

        var anotherClientId = Guid.NewGuid();
        Assert.NotEqual(ApplicationUserId, anotherClientId);

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = anotherClientId.ToString(),
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestInDbWithUnresolvedTrn_ReturnsOkWithPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var slugId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p
            .WithoutTrn()
            .WithSlugId(slugId)
            .WithTrnRequest(ApplicationUserId, requestId));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = person.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Pending",
                trn = (string)null,
                qtsDate = (DateOnly?)null,
                potentialDuplicate = true,
                slugId = slugId
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Get_ValidRequestInCrmWithUnresolvedTrn_ReturnsOkWithPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var slugId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p
            .WithoutTrn()
            .WithTrnRequest(ApplicationUserId, requestId)
            .WithSlugId(slugId));

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Pending",
                trn = (string)null,
                qtsDate = (DateOnly?)null,
                potentialDuplicate = true,
                slugId = slugId
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Get_ValidRequestInDbWithResolvedTrn_ReturnsOkWithCompletedStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var slugId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId)
            .WithTrnRequest(ApplicationUserId, requestId));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = person.ContactId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Completed",
                trn = person.Trn,
                qtsDate = (DateOnly?)null,
                potentialDuplicate = false,
                slugId = slugId
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Get_ValidRequestInCrmWithResolvedTrn_ReturnsOkWithCompletedStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var slugId = Guid.NewGuid().ToString();

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithTrnRequest(ApplicationUserId, requestId)
            .WithSlugId(slugId));

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Completed",
                trn = person.Trn,
                qtsDate = (DateOnly?)null,
                potentialDuplicate = false,
                slugId = slugId
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Get_ForTrnRequestInDbWithTrnToken_ReturnsAccessYourQualificationsLink()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var slugId = Guid.NewGuid().ToString();
        var trnToken = "ABCDEFG1234567";

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithSlugId(slugId)
            .WithTrnRequest(ApplicationUserId, requestId)
            .WithTrnToken(trnToken));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = person.ContactId,
                TrnToken = trnToken
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Completed",
                trn = person.Trn,
                qtsDate = (DateOnly?)null,
                potentialDuplicate = false,
                slugId = slugId,
                accessYourTeachingQualificationsLink = $"https://aytq.com/qualifications/start?trn_token={trnToken}"
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Get_ForTrnRequestInCrmWithTrnToken_ReturnsAccessYourQualificationsLink()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var slugId = Guid.NewGuid().ToString();
        var trnToken = "ABCDEFG1234567";

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithTrnRequest(ApplicationUserId, requestId)
            .WithSlugId(slugId)
            .WithTrnToken(trnToken));

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEqualsAsync(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Completed",
                trn = person.Trn,
                qtsDate = (DateOnly?)null,
                potentialDuplicate = false,
                slugId = slugId,
                accessYourTeachingQualificationsLink = $"https://aytq.com/qualifications/start?trn_token={trnToken}"
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }
}
