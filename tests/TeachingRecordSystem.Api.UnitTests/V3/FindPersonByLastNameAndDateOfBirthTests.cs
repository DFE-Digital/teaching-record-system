using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class FindPersonByLastNameAndDateOfBirthTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_NoPersonMatchesFilter_ReturnsEmptyResult()
    {
        // Arrange
        var command = new FindPersonByLastNameAndDateOfBirthCommand(
            LastName: "NonExistentSurname",
            DateOfBirth: new DateOnly(1990, 1, 1));

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.Equal(0, success.Total);
        Assert.Empty(success.Items);
    }

    [Fact]
    public async Task HandleAsync_PersonMatchesLastNameAndDateOfBirth_ReturnsResult()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName("Testington")
            .WithDateOfBirth(new DateOnly(1985, 3, 20)));

        var command = new FindPersonByLastNameAndDateOfBirthCommand(
            LastName: person.LastName,
            DateOfBirth: person.DateOfBirth);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.Equal(1, success.Total);
        Assert.Single(success.Items);
        Assert.Equal(person.Trn, success.Items.Single().Trn);
    }

    [Fact]
    public async Task HandleAsync_PersonMatchesLastNameButNotDateOfBirth_ReturnsEmptyResult()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithLastName("Testington")
            .WithDateOfBirth(new DateOnly(1985, 3, 20)));

        var command = new FindPersonByLastNameAndDateOfBirthCommand(
            LastName: person.LastName,
            DateOfBirth: new DateOnly(1990, 1, 1));

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.Equal(0, success.Total);
        Assert.Empty(success.Items);
    }
}
