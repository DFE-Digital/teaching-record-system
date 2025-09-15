using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class GetTrnRequestTests : OperationTestBase
{
    [Before(Test)]
    public void ConfigureMocks()
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

    [Test]
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

    [Test]
    public async Task HandleAsync_RequestIsPending_ReturnsPendingStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

        await TestData.CreateApiTrnRequestSupportTaskAsync(
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

    [Test]
    public async Task HandleAsync_RequestIsCompleted_ReturnsTrnAndCompletedStatus()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();
        var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrnRequest(applicationUserId, requestId)
            .WithEmail(TestData.GenerateUniqueEmail()));

        var command = new GetTrnRequestCommand(requestId);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = result.GetSuccess();
        Assert.Equal(TrnRequestStatus.Completed, success.Status);
        Assert.Equal(person.Trn, success.Trn);
        Assert.NotNull(success.AccessYourTeachingQualificationsLink);
    }
}
