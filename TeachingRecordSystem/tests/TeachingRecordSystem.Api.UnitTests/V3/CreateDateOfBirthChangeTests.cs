using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.Api.UnitTests.V3;

[Collection(nameof(DisableParallelization))]
public class CreateDateOfBirthChangeTests : OperationTestBase
{
    public CreateDateOfBirthChangeTests(OperationTestFixture operationTestFixture) : base(operationTestFixture)
    {
    }

    [Fact]
    public Task HandleAsync_PersonDoesNotExist_ReturnsError() =>
        WithHandler<CreateDateOfBirthChangeRequestHandler>(async handler =>
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
        WithHandler<CreateDateOfBirthChangeRequestHandler>(async handler =>
        {
            // Arrange
            var createPersonResult = await TestData.CreatePersonAsync();
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
        WithHandler<CreateDateOfBirthChangeRequestHandler>(async handler =>
        {
            // Arrange
            var createPersonResult = await TestData.CreatePersonAsync();
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
                Assert.Equal(SupportTaskType.ChangeDateOfBirthRequest, supportTask.SupportTaskType);
                var requestData = supportTask.Data as ChangeDateOfBirthRequestData;
                Assert.NotNull(requestData);
                Assert.Equal(command.DateOfBirth, requestData.DateOfBirth);
                Assert.Equal(command.EvidenceFileName, requestData.EvidenceFileName);

                var email = await dbContext.Emails
                    .Where(e => e.EmailAddress == command.EmailAddress)
                    .SingleOrDefaultAsync();
                Assert.NotNull(email);
                Assert.NotNull(email.SentOn);
            });
        });

    private async Task<CreateDateOfBirthChangeRequestCommand> CreateCommand() =>
        new CreateDateOfBirthChangeRequestCommand()
        {
            Trn = await TestData.GenerateTrnAsync(),
            DateOfBirth = TestData.GenerateDateOfBirth(),
            EmailAddress = TestData.GenerateUniqueEmail(),
            EvidenceFileName = "evidence.txt",
            EvidenceFileUrl = Startup.EvidenceFileUrl
        };
}
