using TeachingRecordSystem.Api.V3.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class GetTrnRequestTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_RequestDoesNotExist_ReturnsError()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        var command = new GetTrnRequestCommand(requestId);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.TrnRequestDoesNotExist);
    }

    [Fact]
    public async Task HandleAsync_RequestIsPending_ReturnsPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var applicationUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        await TestData.CreateTrnRequestSupportTaskAsync(
            applicationUserId,
            c => c.WithRequestId(requestId).WithStatus(SupportTaskStatus.Open));

        var command = new GetTrnRequestCommand(requestId);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = result.GetSuccess();
        Assert.Equal(TrnRequestStatus.Pending, success.Status);
        Assert.Null(success.Trn);
        Assert.Null(success.AccessYourTeachingQualificationsLink);
    }

    [Fact]
    public async Task HandleAsync_RequestIsCompleted_ReturnsTrnAndCompletedStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var applicationUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrnRequest(applicationUserId, requestId)
            .WithEmailAddress(TestData.GenerateUniqueEmail()));

        var command = new GetTrnRequestCommand(requestId);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = result.GetSuccess();
        Assert.Equal(TrnRequestStatus.Completed, success.Status);
        Assert.Equal(person.Trn, success.Trn);
        Assert.NotNull(success.AccessYourTeachingQualificationsLink);
    }

    [Fact]
    public async Task HandleAsync_RequestIsRejected_ReturnsError()
    {
        // Arrange
        var applicationUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        var task = await TestData.CreateTrnRequestSupportTaskAsync(applicationUserId);

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.SupportTasks.Attach(task.SupportTask);
            dbContext.TrnRequestMetadata.Attach(task.TrnRequest);

            task.SupportTask.Status = SupportTaskStatus.Closed;
            task.TrnRequest.Status = TrnRequestStatus.Rejected;

            await dbContext.SaveChangesAsync();
        });

        var command = new GetTrnRequestCommand(task.TrnRequest.RequestId);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.UnsupportedTrnRequestStatus);
    }

    [Fact]
    public async Task HandleAsync_RequestIsDormantAndDormantRequestsNotSupported_ReturnsError()
    {
        // Arrange
        var applicationUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        var trnRequest = await TestData.CreateDormantTrnRequestAsync(applicationUserId);

        var command = new GetTrnRequestCommand(trnRequest.RequestId, GetTrnRequestCommandOptions.None);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.UnsupportedTrnRequestStatus);
    }

    [Fact]
    public async Task HandleAsync_RequestIsDormantAndDormantRequestsAreSupported_ReturnsDormantStatus()
    {
        // Arrange
        var applicationUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        var trnRequest = await TestData.CreateDormantTrnRequestAsync(applicationUserId);

        var command = new GetTrnRequestCommand(trnRequest.RequestId, GetTrnRequestCommandOptions.SupportsDormantRequests);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = result.GetSuccess();
        Assert.Equal(TrnRequestStatus.Dormant, success.Status);
        Assert.Null(success.Trn);
        Assert.Null(success.AccessYourTeachingQualificationsLink);
    }
}
