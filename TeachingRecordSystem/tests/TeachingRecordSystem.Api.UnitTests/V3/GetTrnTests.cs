using System.Diagnostics;
using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class GetTrnTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var trn = "0000000";
        var command = new GetTrnCommand(trn);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_PersonExistsButIsNotActive_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));

        var command = new GetTrnCommand(person.Trn!);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.RecordIsNotActive);
    }

    [Fact]
    public async Task HandleAsync_PersonIsMerged_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var anotherPerson = await TestData.CreatePersonAsync();

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.Status, _ => PersonStatus.Deactivated)
                    .SetProperty(p => p.MergedWithPersonId, _ => anotherPerson.PersonId)));

        var command = new GetTrnCommand(person.Trn!);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        var error = AssertError(result, ApiError.ErrorCodes.RecordIsMerged);
        Assert.Collection(
            error.Data,
            kvp =>
            {
                Assert.Equal(ApiError.DataKeys.MergedWithTrn, kvp.Key);
                Assert.Equal(anotherPerson.Trn, kvp.Value);
            });
    }

    [Fact]
    public async Task HandleAsync_PersonExistsAndIsActive_ReturnsSuccess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        Debug.Assert(person.Person.Status is PersonStatus.Active);
        var command = new GetTrnCommand(person.Trn!);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);
    }
}
