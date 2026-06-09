using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class FindPersonsByTrnAndDateOfBirthTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_EmptyInput_ReturnsEmptyResult()
    {
        // Arrange
        var command = new FindPersonsByTrnAndDateOfBirthCommand([]);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.Equal(0, success.Total);
        Assert.Empty(success.Items);
    }

    [Fact]
    public async Task HandleAsync_PersonWithMatchingTrnAndDateOfBirth_ReturnsResult()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithDateOfBirth(new DateOnly(1988, 7, 10)));

        var command = new FindPersonsByTrnAndDateOfBirthCommand(
            [(person.Trn, person.DateOfBirth)]);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.Equal(1, success.Total);
        Assert.Single(success.Items);
        Assert.Equal(person.Trn, success.Items.Single().Trn);
    }

    [Fact]
    public async Task HandleAsync_PersonWithMatchingTrnButDifferentDateOfBirth_IsExcluded()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithDateOfBirth(new DateOnly(1988, 7, 10)));

        var command = new FindPersonsByTrnAndDateOfBirthCommand(
            [(person.Trn, new DateOnly(1990, 1, 1))]);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.Equal(0, success.Total);
        Assert.Empty(success.Items);
    }

    [Fact]
    public async Task HandleAsync_MixOfMatchingAndNonMatchingPersons_ReturnsOnlyMatches()
    {
        // Arrange
        var matchingPerson = await TestData.CreatePersonAsync(p => p.WithDateOfBirth(new DateOnly(1988, 7, 10)));
        var nonMatchingPerson = await TestData.CreatePersonAsync(p => p.WithDateOfBirth(new DateOnly(1990, 3, 15)));

        var command = new FindPersonsByTrnAndDateOfBirthCommand(
            [
                (matchingPerson.Trn, matchingPerson.DateOfBirth),
                (nonMatchingPerson.Trn, new DateOnly(2000, 1, 1))
            ]);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var success = AssertSuccess(result);
        Assert.Equal(1, success.Total);
        Assert.Single(success.Items);
        Assert.Equal(matchingPerson.Trn, success.Items.Single().Trn);
    }
}
