using System.Diagnostics;
using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class ActivateTrnRequestTests(OperationTestFixture operationTestFixture)
    : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task TrnRequestDoesNotExist_ReturnsError()
    {
        // Arrange
        var trnRequestId = Guid.NewGuid().ToString();

        var command = new ActivateTrnRequestCommand(trnRequestId);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.TrnRequestDoesNotExist);
    }

    [Fact]
    public async Task TrnRequestIsNotDormant_ReturnsResultWithWasActivatedFalse()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var applicationUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        await TestData.CreatePersonAsync(p => p.WithTrnRequest(applicationUserId, requestId));

        var command = new ActivateTrnRequestCommand(requestId);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.False(success.WasActivated);
    }

    [Fact]
    public async Task TrnRequestIsDormant_ActivatesRequestAndReturnsResultWithWasActivatedTrue()
    {
        // Arrange
        var applicationUserId = CurrentUserProvider.GetCurrentApplicationUserId();
        var trnRequest = await TestData.CreateDormantTrnRequestAsync(applicationUserId);
        Debug.Assert(trnRequest.Status is TrnRequestStatus.Dormant);

        var command = new ActivateTrnRequestCommand(trnRequest.RequestId);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.True(success.WasActivated);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedTrnRequest = await dbContext.TrnRequestMetadata.FindAsync(trnRequest.ApplicationUserId, trnRequest.RequestId);
            Assert.NotEqual(TrnRequestStatus.Dormant, updatedTrnRequest!.Status);
        });
    }
}
