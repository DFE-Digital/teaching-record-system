using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;

namespace TeachingRecordSystem.Api.UnitTests.V3;

[Collection(nameof(DisableParallelization))]
public class GetTrnRequestTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture), IAsyncLifetime
{
    [Fact]
    public Task HandleAsync_RequestDoesNotExist_ReturnsError() =>
        WithHandler<GetTrnRequestHandler>(async handler =>
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();

            var command = new GetTrnRequestCommand(requestId);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.TrnRequestDoesNotExist);
        });

    [Fact]
    public Task HandleAsync_RequestIsPending_ReturnsPendingStatus() =>
        WithHandler<GetTrnRequestHandler>(async handler =>
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

            await TestData.CreateApiTrnRequestSupportTaskAsync(
                applicationUserId,
                c => c.WithRequestId(requestId).WithStatus(SupportTaskStatus.Open));

            var command = new GetTrnRequestCommand(requestId);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = result.GetSuccess();
            Assert.Equal(TrnRequestStatus.Pending, success.Status);
            Assert.Null(success.Trn);
            Assert.Null(success.AccessYourTeachingQualificationsLink);
        });

    [Fact]
    public Task HandleAsync_RequestIsCompleted_ReturnsTrnAndCompletedStatus() =>
        WithHandler<GetTrnRequestHandler>(async handler =>
        {
            // Arrange
            var requestId = Guid.NewGuid().ToString();
            var (applicationUserId, _) = CurrentUserProvider.GetCurrentApplicationUser();

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithTrnRequest(applicationUserId, requestId)
                .WithEmail(TestData.GenerateUniqueEmail()));

            var command = new GetTrnRequestCommand(requestId);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = result.GetSuccess();
            Assert.Equal(TrnRequestStatus.Completed, success.Status);
            Assert.Equal(person.Trn, success.Trn);
            Assert.NotNull(success.AccessYourTeachingQualificationsLink);
        });

    public async Task InitializeAsync()
    {
        // Any existing Contacts will affect our duplicate matching; clear them all out before every test
        await OperationTestFixture.DbFixture.DbHelper.DeleteAllPersonsAsync();
        XrmFakedContext.DeleteAllEntities<Contact>();

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

    public Task DisposeAsync() => Task.CompletedTask;
}
