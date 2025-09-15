using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class CreateNameChangeTests : OperationTestBase
{
    [Test]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = await CreateCommand();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Test]
    public async Task HandleAsync_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var command = await CreateCommand() with
        {
            Trn = createPersonResult.Trn!,
            EvidenceFileUrl = "https://nonexistenturl.com"
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.SpecifiedResourceUrlDoesNotExist);
    }

    [Test]
    public async Task HandleAsync_ValidRequest_CreatesSupportTaskAndSendsEmailAndReturnsTicketNumber()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var command = await CreateCommand() with
        {
            Trn = createPersonResult.Trn!
        };
        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.NotEmpty(success.CaseNumber);

        await WithDbContextAsync(async dbContext =>
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
    }

    private async Task<CreateNameChangeRequestCommand> CreateCommand() =>
        new CreateNameChangeRequestCommand
        {
            Trn = await TestData.GenerateTrnAsync(),
            FirstName = TestData.GenerateFirstName(),
            MiddleName = TestData.GenerateMiddleName(),
            LastName = TestData.GenerateLastName(),
            EmailAddress = TestData.GenerateUniqueEmail(),
            EvidenceFileName = "evidence.jpg",
            EvidenceFileUrl = EvidenceFilesHttpClientHelper.EvidenceFileUrl
        };
}
