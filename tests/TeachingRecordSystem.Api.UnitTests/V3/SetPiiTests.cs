using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public class SetPiiTests(OperationTestFixture operationTestFixture) : OperationTestBase(operationTestFixture)
{
    [Fact]
    public async Task HandleAsync_PersonDoesNotExist_ReturnsError()
    {
        // Arrange
        var command = CreateCommand("0000000");

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PersonNotFound);
    }

    [Fact]
    public async Task HandleAsync_PersonDoesNotAllowDetailsUpdates_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // AllowDetailsUpdatesFromSourceApplication defaults to false
        var command = CreateCommand(person.Trn);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PiiUpdatesForbidden);
    }

    [Fact]
    public async Task HandleAsync_PersonSourceApplicationUserIdDoesNotMatchCurrentUser_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.AllowDetailsUpdatesFromSourceApplication, _ => true)
                    .SetProperty(p => p.SourceApplicationUserId, _ => Guid.NewGuid())));

        var command = CreateCommand(person.Trn);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PiiUpdatesForbidden);
    }

    [Fact]
    public async Task HandleAsync_PersonHasQts_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var currentUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.AllowDetailsUpdatesFromSourceApplication, _ => true)
                    .SetProperty(p => p.SourceApplicationUserId, _ => currentUserId)));

        var command = CreateCommand(person.Trn);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PiiUpdatesForbiddenPersonHasQts);
    }

    [Fact]
    public async Task HandleAsync_PersonHasEyts_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEyts());
        var currentUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.AllowDetailsUpdatesFromSourceApplication, _ => true)
                    .SetProperty(p => p.SourceApplicationUserId, _ => currentUserId)));

        var command = CreateCommand(person.Trn);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertError(result, ApiError.ErrorCodes.PiiUpdatesForbiddenPersonHasEyts);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_UpdatesPersonAndReturnsSuccess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var currentUserId = CurrentUserProvider.GetCurrentApplicationUserId();

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == person.PersonId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(p => p.AllowDetailsUpdatesFromSourceApplication, _ => true)
                    .SetProperty(p => p.SourceApplicationUserId, _ => currentUserId)));

        var command = CreateCommand(person.Trn);

        // Act
        var result = await ExecuteCommandAsync(command);

        // Assert
        AssertSuccess(result);

        var updatedPerson = await WithDbContextAsync(dbContext =>
            dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));

        Assert.Equal(command.FirstName, updatedPerson.FirstName);
        Assert.Equal(command.LastName, updatedPerson.LastName);
        Assert.Equal(command.DateOfBirth, updatedPerson.DateOfBirth);
    }

    private static SetPiiCommand CreateCommand(string trn) =>
        new SetPiiCommand()
        {
            Trn = trn,
            FirstName = "Jane",
            MiddleName = null,
            LastName = "Smith",
            DateOfBirth = new DateOnly(1990, 5, 15),
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
        };
}
