using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class GetContactsByDateOfBirthTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetContactsByDateOfBirthTests(CrmClientFixture crmClientFixture)
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
        var name = "smith";
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var maxRecordCount = 3;

        var person1 = await _dataScope.TestData.CreatePerson(b => b.WithDateOfBirth(dateOfBirth));
        var person2 = await _dataScope.TestData.CreatePerson(b => b.WithDateOfBirth(dateOfBirth));
        var person3 = await _dataScope.TestData.CreatePerson(b => b.WithDateOfBirth(dateOfBirth));

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new GetContactsByDateOfBirthQuery(dateOfBirth, maxRecordCount, new ColumnSet()));

        // Assert
        Assert.NotNull(results);
        Assert.Equal(maxRecordCount, results.Length);
    }
}
