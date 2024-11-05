using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetActiveContactsByLastNameAndDateOfBirthTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetActiveContactsByLastNameAndDateOfBirthTests(CrmClientFixture crmClientFixture)
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

        var person1 = await _dataScope.TestData.CreatePerson(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth));
        var person2 = await _dataScope.TestData.CreatePerson(p => p.WithTrn().WithLastName(lastName).WithDateOfBirth(dateOfBirth));

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactsByLastNameAndDateOfBirthQuery(lastName, dateOfBirth, new ColumnSet()));

        // Assert
        Assert.NotNull(results);
        Assert.Contains(results, r => r.Id == person1.ContactId);
        Assert.Contains(results, r => r.Id == person2.ContactId);
    }
}
