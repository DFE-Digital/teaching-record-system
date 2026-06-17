using TeachingRecordSystem.Api.V3.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetCpdInductionStatusTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Theory]
    [InlineData(InductionStatus.None)]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task HandleAsync_InvalidStatus_ReturnsError(InductionStatus status)
    {
        // Arrange
        var command = new SetCpdInductionStatusCommand(
            Trn: "0000000",
            Status: status,
            StartDate: null,
            CompletedDate: null,
            CpdModifiedOn: DateTime.UtcNow);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InvalidInductionStatus);
    }

    [Fact]
    public async Task HandleAsync_StatusRequiresStartDateAndStartDateIsNull_ReturnsError()
    {
        // Arrange
        var command = new SetCpdInductionStatusCommand(
            Trn: "0000000",
            Status: InductionStatus.InProgress,
            StartDate: null,
            CompletedDate: null,
            CpdModifiedOn: DateTime.UtcNow);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InductionStartDateIsRequired);
    }

    [Fact]
    public async Task HandleAsync_StatusDoesNotRequireStartDateAndStartDateProvided_ReturnsError()
    {
        // Arrange
        var command = new SetCpdInductionStatusCommand(
            Trn: "0000000",
            Status: InductionStatus.RequiredToComplete,
            StartDate: new DateOnly(2024, 1, 1),
            CompletedDate: null,
            CpdModifiedOn: DateTime.UtcNow);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InductionStartDateIsNotPermitted);
    }

    [Fact]
    public async Task HandleAsync_StatusRequiresCompletedDateAndCompletedDateIsNull_ReturnsError()
    {
        // Arrange
        var command = new SetCpdInductionStatusCommand(
            Trn: "0000000",
            Status: InductionStatus.Passed,
            StartDate: new DateOnly(2024, 1, 1),
            CompletedDate: null,
            CpdModifiedOn: DateTime.UtcNow);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InductionCompletedDateIsRequired);
    }

    [Fact]
    public async Task HandleAsync_StatusDoesNotRequireCompletedDateAndCompletedDateProvided_ReturnsError()
    {
        // Arrange
        var command = new SetCpdInductionStatusCommand(
            Trn: "0000000",
            Status: InductionStatus.InProgress,
            StartDate: new DateOnly(2024, 1, 1),
            CompletedDate: new DateOnly(2024, 6, 1),
            CpdModifiedOn: DateTime.UtcNow);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.InductionCompletedDateIsNotPermitted);
    }

    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = new SetCpdInductionStatusCommand(
            Trn: "0000000",
            Status: InductionStatus.RequiredToComplete,
            StartDate: null,
            CompletedDate: null,
            CpdModifiedOn: DateTime.UtcNow);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_PersonDoesNotHaveQts_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var command = new SetCpdInductionStatusCommand(
            Trn: person.Trn,
            Status: InductionStatus.RequiredToComplete,
            StartDate: null,
            CompletedDate: null,
            CpdModifiedOn: DateTime.UtcNow);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonDoesNotHaveQts);
    }

    [Fact]
    public async Task HandleAsync_RequestIsStale_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var existingModifiedOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.CpdInductionCpdModifiedOn, _ => existingModifiedOn)));

        var command = new SetCpdInductionStatusCommand(
            Trn: person.Trn,
            Status: InductionStatus.RequiredToComplete,
            StartDate: null,
            CompletedDate: null,
            CpdModifiedOn: existingModifiedOn.AddDays(-1));

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.StaleRequest);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var command = new SetCpdInductionStatusCommand(
            Trn: person.Trn,
            Status: InductionStatus.InProgress,
            StartDate: new DateOnly(2024, 1, 1),
            CompletedDate: null,
            CpdModifiedOn: DateTime.UtcNow);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);
    }
}
