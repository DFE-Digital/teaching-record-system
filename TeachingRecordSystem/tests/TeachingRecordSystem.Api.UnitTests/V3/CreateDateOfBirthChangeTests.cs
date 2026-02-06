using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class CreateDateOfBirthChangeTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = await CreateCommand();

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_EvidenceFileDoesNotExist_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var command = await CreateCommand() with
        {
            Trn = person.Trn,
            EvidenceFileUrl = "https://nonexistenturl.com"
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.SpecifiedResourceUrlDoesNotExist);
    }

    [Fact]
    public async Task HandleAsync_PersonHasOpenChangeDateOfBirthRequest_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithStatus(SupportTaskStatus.Open));

        var command = await CreateCommand() with
        {
            Trn = person.Trn
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.OpenDateOfBirthChangeRequestAlreadyExists);
    }

    [Fact]
    public async Task HandleAsync_PersonHasInProgressChangeDateOfBirthRequest_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithStatus(SupportTaskStatus.InProgress));

        var command = await CreateCommand() with
        {
            Trn = person.Trn
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.OpenDateOfBirthChangeRequestAlreadyExists);
    }

    [Fact]
    public async Task HandleAsync_PersonHasClosedChangeDateOfBirthRequest_CreatesSupportTaskSuccessfully()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithStatus(SupportTaskStatus.Closed));

        var command = await CreateCommand() with
        {
            Trn = person.Trn
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.NotEmpty(success.CaseNumber);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesSupportTaskAndSendsEmailAndReturnsTicketNumber()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var command = await CreateCommand() with
        {
            Trn = person.Trn
        };

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.NotEmpty(success.CaseNumber);

        await WithDbContextAsync(async dbContext =>
        {
            var supportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t => t.PersonId == person.PersonId);
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

        Events.AssertProcessesCreated(t =>
        {
            var process = t.ProcessContext;
            Assert.Equal(ProcessType.ChangeOfDateOfBirthRequestCreating, process.ProcessType);
            Assert.Equal(CurrentUserProvider.GetCurrentApplicationUserId(), process.Process.UserId);

            Assert.Collection(
                t.Events,
                e => Assert.IsType<EmailSentEvent>(e),
                e => Assert.IsType<SupportTaskCreatedEvent>(e));
        });
    }

    private async Task<CreateDateOfBirthChangeRequestCommand> CreateCommand() =>
        new CreateDateOfBirthChangeRequestCommand()
        {
            Trn = await TestData.GenerateTrnAsync(),
            DateOfBirth = TestData.GenerateDateOfBirth(),
            EmailAddress = TestData.GenerateUniqueEmail(),
            EvidenceFileName = "evidence.txt",
            EvidenceFileUrl = EvidenceFilesHttpClientHelper.EvidenceFileUrl
        };
}
