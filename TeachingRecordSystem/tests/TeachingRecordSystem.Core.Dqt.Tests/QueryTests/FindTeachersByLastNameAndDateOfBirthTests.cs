using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;
public class FindTeachersByLastNameAndDateOfBirthTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public FindTeachersByLastNameAndDateOfBirthTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task ReturnsMatchingContactsFromCrm()
    {
        // Arrange
        var lastName = "Smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);

        var person1 = await _dataScope.TestData.CreatePerson(b => b.WithLastName(lastName).WithDateOfBirth(dateOfBirth));
        var person2 = await _dataScope.TestData.CreatePerson(b => b.WithLastName(lastName).WithDateOfBirth(dateOfBirth));

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new FindTeachersByLastNameAndDateOfBirthQuery(lastName, dateOfBirth, new ColumnSet()));

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count());
    }
}
