#nullable disable
using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

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

        await WithDbContext(async dbContext =>
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
        var teacherId = Guid.NewGuid();
        var slugId = Guid.NewGuid().ToString();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), true))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_TRN = null,
                dfeta_SlugId = slugId
            });

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEquals(
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
        var trnRequestId = TrnRequestHelper.GetCrmTrnRequestId(ApplicationUserId, requestId);
        var createPersonResult = await TestData.CreatePerson(p => p.WithoutTrn().WithTrnRequestId(trnRequestId).WithSlugId(slugId));

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(createPersonResult.ContactId, /* resolveMerges: */ It.IsAny<string[]>(), true))
            .ReturnsAsync(new Contact()
            {
                Id = createPersonResult.ContactId,
                dfeta_TRN = null,
                dfeta_SlugId = slugId
            });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEquals(
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
        var teacherId = Guid.NewGuid();
        var trn = "1234567";
        var slugId = Guid.NewGuid().ToString();

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), true))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_TRN = trn,
                dfeta_SlugId = slugId
            });

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Completed",
                trn = trn,
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
        var trnRequestId = TrnRequestHelper.GetCrmTrnRequestId(ApplicationUserId, requestId);
        var createPersonResult = await TestData.CreatePerson(p => p.WithTrn().WithTrnRequestId(trnRequestId));

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(createPersonResult.ContactId, /* resolveMerges: */ It.IsAny<string[]>(), true))
            .ReturnsAsync(new Contact()
            {
                Id = createPersonResult.ContactId,
                dfeta_TRN = createPersonResult.Trn,
                dfeta_SlugId = slugId
            });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Completed",
                trn = createPersonResult.Trn,
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
        var teacherId = Guid.NewGuid();
        var trn = "1234567";
        var trnToken = "ABCDEFG1234567";
        var slugId = Guid.NewGuid().ToString();
        var qtsDate = new DateTime(2020, 10, 03);

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(teacherId, /* resolveMerges: */ It.IsAny<string[]>(), true))
            .ReturnsAsync(new Contact()
            {
                Id = teacherId,
                dfeta_TRN = trn,
                dfeta_SlugId = slugId,
                dfeta_QTSDate = qtsDate
            });

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = ApplicationUserId.ToString(),
                RequestId = requestId,
                TeacherId = teacherId,
                TrnToken = trnToken
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Completed",
                trn = trn,
                qtsDate = qtsDate.ToDateOnlyWithDqtBstFix(true),
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
        var trnToken = "ABCDEFG1234567";
        var qtsDate = new DateOnly(2020, 10, 03);
        var trnRequestId = TrnRequestHelper.GetCrmTrnRequestId(ApplicationUserId, requestId);
        var createPersonResult = await TestData.CreatePerson(p => p.WithTrn().WithTrnRequestId(trnRequestId).WithQts(qtsDate).WithTrnToken(trnToken));

        DataverseAdapterMock
            .Setup(mock => mock.GetTeacher(createPersonResult.ContactId, /* resolveMerges: */ It.IsAny<string[]>(), true))
            .ReturnsAsync(new Contact()
            {
                Id = createPersonResult.ContactId,
                dfeta_TRN = createPersonResult.Trn,
                dfeta_SlugId = null,
                dfeta_QTSDate = qtsDate.ToDateTimeWithDqtBstFix(isLocalTime: true)
            });

        // Act
        var response = await GetHttpClientWithApiKey().GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                requestId = requestId,
                status = "Completed",
                trn = createPersonResult.Trn,
                qtsDate = qtsDate,
                potentialDuplicate = false,
                slugId = (string)null,
                accessYourTeachingQualificationsLink = $"https://aytq.com/qualifications/start?trn_token={trnToken}"
            },
            expectedStatusCode: StatusCodes.Status200OK);
    }
}
