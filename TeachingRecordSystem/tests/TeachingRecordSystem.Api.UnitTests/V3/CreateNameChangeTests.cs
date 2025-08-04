using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.Api.UnitTests.V3;

[Collection(nameof(DisableParallelization))]
public class CreateNameChangeTests : OperationTestBase
{
    public CreateNameChangeTests(OperationTestFixture operationTestFixture) : base(operationTestFixture)
    {
        FeatureProvider.Features.Add(FeatureNames.ChangeRequestsInTrs);
    }

    [Fact]
    public Task HandleAsync_PersonDoesNotExist_ReturnsError() =>
        WithHandler<CreateNameChangeRequestHandler>(async handler =>
        {
            // Arrange
            var command = await CreateCommand();

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.PersonNotFound);
        });

    [Fact]
    public Task HandleAsync_EvidenceFileDoesNotExist_ReturnsError() =>
        WithHandler<CreateNameChangeRequestHandler>(async handler =>
        {
            // Arrange
            var createPersonResult = await TestData.CreatePersonAsync(p => p.WithTrn());
            var command = await CreateCommand() with
            {
                Trn = createPersonResult!.Trn!,
                EvidenceFileUrl = "https://nonexistenturl.com"
            };

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.SpecifiedResourceUrlDoesNotExist);
        });

    [Fact]
    public Task HandleAsync_ValidRequest_CreatesSupportTaskAndSendsEmailAndReturnsTicketNumber() =>
        WithHandler<CreateNameChangeRequestHandler>(async handler =>
        {
            // Arrange
            var createPersonResult = await TestData.CreatePersonAsync(p => p.WithTrn());
            var command = await CreateCommand() with
            {
                Trn = createPersonResult!.Trn!
            };
            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);
            Assert.NotEmpty(success.CaseNumber);

            await DbFixture.WithDbContextAsync(async dbContext =>
            {
                var supportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t => t.PersonId == createPersonResult.PersonId);
                Assert.NotNull(supportTask);
                Assert.Equal(SupportTaskType.ChangeNameRequest, supportTask.SupportTaskType);
                var requestData = supportTask.Data as ChangeNameRequestData;
                Assert.NotNull(requestData);
                Assert.Equal(command.FirstName, requestData.FirstName);
                Assert.Equal(command.MiddleName, requestData.MiddleName);
                Assert.Equal(command.LastName, requestData.LastName);
                Assert.Equal(command.EvidenceFileName, requestData.EvidenceFileName);

                var email = await dbContext.Emails
                    .Where(e => e.EmailAddress == command.EmailAddress)
                    .SingleOrDefaultAsync();
                Assert.NotNull(email);
                Assert.NotNull(email.SentOn);
            });
        });

    private async Task<CreateNameChangeRequestCommand> CreateCommand() =>
        new CreateNameChangeRequestCommand
        {
            Trn = await TestData.GenerateTrnAsync(),
            FirstName = TestData.GenerateFirstName(),
            MiddleName = TestData.GenerateMiddleName(),
            LastName = TestData.GenerateLastName(),
            EmailAddress = TestData.GenerateUniqueEmail(),
            EvidenceFileName = "evidence.jpg",
            EvidenceFileUrl = Startup.EvidenceFileUrl
        };
}
