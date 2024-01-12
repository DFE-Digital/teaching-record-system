#nullable disable
using System.Net;
using TeachingRecordSystem.Api.Tests.Attributes;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

public class GetTrnRequestTests : TestBase
{
    public GetTrnRequestTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.UpdatePerson });
    }

    [Theory, RoleNamesData(new[] { ApiRoles.UpdatePerson })]
    public async Task GetTrnRequest_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);
        var requestId = Guid.NewGuid().ToString();

        // Act
        var response = await HttpClientWithApiKey.GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_trn_request_with_specified_id_does_not_exist_returns_notfound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        // Act
        var response = await HttpClientWithApiKey.GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Given_trn_request_with_specified_id_does_not_exist_for_current_client_returns_notfound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();

        var anotherClientId = "ANOTHER-CLIENT";
        Assert.NotEqual(ClientId, anotherClientId);

        await WithDbContext(async dbContext =>
        {
            dbContext.Add(new TrnRequest()
            {
                ClientId = anotherClientId,
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await HttpClientWithApiKey.GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Given_valid_pending_trn_request_returns_ok_with_pending_status()
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
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await HttpClientWithApiKey.GetAsync($"v2/trn-requests/{requestId}");

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
    public async Task Given_valid_completed_trn_request_returns_ok_with_completed_status_and_trn()
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
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = teacherId
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await HttpClientWithApiKey.GetAsync($"v2/trn-requests/{requestId}");

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
    public async Task Get_ForTrnRequestWithTrnToken_ReturnsAccessYourQualificationsLink()
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
                ClientId = ClientId,
                RequestId = requestId,
                TeacherId = teacherId,
                TrnToken = trnToken
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        var response = await HttpClientWithApiKey.GetAsync($"v2/trn-requests/{requestId}");

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
}
