using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetDeceasedTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = new SetDeceasedCommand(
            Trn: "0000000",
            DateOfDeath: new DateOnly(2024, 1, 1));

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_DeactivatesPersonAndReturnsSuccess()
    {
        // Arrange
        var dateOfDeath = new DateOnly(2024, 6, 15);
        var person = await TestData.CreatePersonAsync();

        var command = new SetDeceasedCommand(
            Trn: person.Trn,
            DateOfDeath: dateOfDeath);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var updatedPerson = await WithDbContextAsync(dbContext =>
            dbContext.Persons.IgnoreQueryFilters().SingleAsync(p => p.PersonId == person.PersonId));

        Assert.Equal(PersonStatus.Deactivated, updatedPerson.Status);
        Assert.Equal(dateOfDeath, updatedPerson.DateOfDeath);
    }
}
