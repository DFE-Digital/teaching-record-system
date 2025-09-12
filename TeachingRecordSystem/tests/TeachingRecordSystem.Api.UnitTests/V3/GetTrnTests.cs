using System.Diagnostics;
using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class GetTrnTests : OperationTestBase
{
    [Test]
    public Task HandleAsync_PersonDoesNotExist_ReturnsError() =>
        WithHandler<GetTrnHandler>(async handler =>
        {
            // Arrange
            var trn = "0000000";
            var command = new GetTrnCommand(trn);

            // Act
            var result = await handler.ExecuteAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.PersonNotFound);
        });

    [Test]
    public Task HandleAsync_PersonExistsButIsNotActive_ReturnsError() =>
        WithHandler<GetTrnHandler>(async handler =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();

            await WithDbContextAsync(dbContext =>
                dbContext.Persons
                    .Where(p => p.PersonId == person.PersonId)
                    .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));

            var command = new GetTrnCommand(person.Trn!);

            // Act
            var result = await handler.ExecuteAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.RecordIsNotActive);
        });

    [Test]
    public Task HandleAsync_PersonIsMerged_ReturnsError() =>
        WithHandler<GetTrnHandler>(async handler =>
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
            var result = await handler.ExecuteAsync(command);

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

    [Test]
    public Task HandleAsync_PersonExistsAndIsActive_ReturnsSuccess() =>
        WithHandler<GetTrnHandler>(async handler =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            Debug.Assert(person.Person.Status is PersonStatus.Active);
            var command = new GetTrnCommand(person.Trn!);

            // Act
            var result = await handler.ExecuteAsync(command);

            // Assert
            AssertSuccess(result);
        });
}
