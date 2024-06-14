using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetActiveContactDetailByIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetActiveContactDetailByIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_WithContactIdForNonExistentContact_ReturnsNull()
    {
        // Arrange        
        var contactId = Guid.NewGuid();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(contactId, new ColumnSet()));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WhenCalled_WithContactIdForExistingContact_ReturnsContactDetail()
    {
        // Arrange        
        var person = await _dataScope.TestData.CreatePerson();

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(person.ContactId, new ColumnSet()));

        // Assert
        Assert.NotNull(results);
        Assert.Equal(results.Contact.Id, person.ContactId);
        Assert.Empty(results.PreviousNames);
    }

    [Fact]
    public async Task WhenCalled_WithContactIdForExistingContactWithPreviousName_ReturnsContactDetailIncludingPreviousNames()
    {
        // Arrange
        var updatedFirstName = _dataScope.TestData.GenerateFirstName();
        var updatedMiddleName = _dataScope.TestData.GenerateMiddleName();
        var updatedLastName = _dataScope.TestData.GenerateLastName();
        var person = await _dataScope.TestData.CreatePerson();
        await _dataScope.TestData.UpdatePerson(b => b.WithPersonId(person.ContactId).WithUpdatedName(updatedFirstName, updatedMiddleName, updatedLastName));

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(person.ContactId, new ColumnSet()));

        // Assert
        Assert.NotNull(results);
        Assert.Equal(results.Contact.Id, person.ContactId);
        Assert.Equal(3, results.PreviousNames.Length);
    }
}
