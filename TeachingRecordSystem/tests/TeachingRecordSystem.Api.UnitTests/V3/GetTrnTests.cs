using System.Diagnostics;
using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class GetTrnTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public Task HandleAsync_PersonDoesNotExist_ReturnsError() =>
        WithHandler<GetTrnHandler>(async handler =>
        {
            // Arrange
            var trn = "0000000";
            var command = new GetTrnCommand(trn);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.PersonNotFound);
        });

    [Fact]
    public Task HandleAsync_PersonExistsButIsNotActive_ReturnsError() =>
        WithHandler<GetTrnHandler>(async handler =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();

            await DbFixture.WithDbContextAsync(dbContext =>
                dbContext.Persons
                    .Where(p => p.PersonId == person.PersonId)
                    .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));

            var command = new GetTrnCommand(person.Trn!);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.RecordIsNotActive);
        });

    [Fact]
    public Task HandleAsync_PersonIsMerged_ReturnsError() =>
        WithHandler<GetTrnHandler>(async handler =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            var anotherPerson = await TestData.CreatePersonAsync();

            await DbFixture.WithDbContextAsync(dbContext =>
                dbContext.Persons
                    .Where(p => p.PersonId == person.PersonId)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(p => p.Status, _ => PersonStatus.Deactivated)
                        .SetProperty(p => p.MergedWithPersonId, _ => anotherPerson.PersonId)));

            var command = new GetTrnCommand(person.Trn!);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var error = AssertError(result, ApiError.ErrorCodes.RecordIsMerged);
            Assert.Collection(
                error.Data,
                kvp =>
                {
                    Assert.Equal(ApiError.DataKeys.MergedWithTrn, kvp.Key);
                    Assert.Equal(anotherPerson.Trn, kvp.Value);
                });
        });

    [Fact]
    public Task HandleAsync_PersonExistsAndIsActive_ReturnsSuccess() =>
        WithHandler<GetTrnHandler>(async handler =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            Debug.Assert(person.Person.Status is PersonStatus.Active);
            var command = new GetTrnCommand(person.Trn!);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertSuccess(result);
        });
}
