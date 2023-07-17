#nullable disable
using System.Net;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.Tests.V2.Operations;

[TestClass]
public class GetTrnRequestTests : ApiTestBase
{
    public GetTrnRequestTests(ApiFixture apiFixture) : base(apiFixture)
    {
    }

    [Test]
    public async Task Given_trn_request_with_specified_id_does_not_exist_returns_notfound()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        // Act
        var response = await HttpClientWithApiKey.GetAsync($"v2/trn-requests/{requestId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
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

    [Test]
    public async Task Given_valid_pending_trn_request_returns_ok_with_pending_status()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var slugId = Guid.NewGuid().ToString();

        DataverseAdapter
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

    [Test]
    public async Task Given_valid_completed_trn_request_returns_ok_with_completed_status_and_trn()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var teacherId = Guid.NewGuid();
        var trn = "1234567";
        var slugId = Guid.NewGuid().ToString();

        DataverseAdapter
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
}
