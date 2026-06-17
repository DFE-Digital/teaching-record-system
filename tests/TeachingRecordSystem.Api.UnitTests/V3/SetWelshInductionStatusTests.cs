using TeachingRecordSystem.Api.V3.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetWelshInductionStatusTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = new SetWelshInductionStatusCommand(
            Trn: "0000000",
            Passed: false,
            StartDate: new DateOnly(2024, 1, 1),
            CompletedDate: new DateOnly(2024, 6, 1));

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

        var command = new SetWelshInductionStatusCommand(
            Trn: person.Trn,
            Passed: false,
            StartDate: new DateOnly(2024, 1, 1),
            CompletedDate: new DateOnly(2024, 6, 1));

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonDoesNotHaveQts);
    }

    [Fact]
    public async Task HandleAsync_PersonPassed_ReturnsSuccess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var command = new SetWelshInductionStatusCommand(
            Trn: person.Trn,
            Passed: true,
            StartDate: new DateOnly(2024, 1, 1),
            CompletedDate: new DateOnly(2024, 6, 1));

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);
    }

    [Fact]
    public async Task HandleAsync_PersonNotPassed_ReturnsSuccess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var command = new SetWelshInductionStatusCommand(
            Trn: person.Trn,
            Passed: false,
            StartDate: new DateOnly(2024, 1, 1),
            CompletedDate: new DateOnly(2024, 6, 1));

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);
    }
}
