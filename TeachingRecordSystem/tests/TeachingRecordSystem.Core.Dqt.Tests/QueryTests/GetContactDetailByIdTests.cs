using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class GetContactDetailByIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetContactDetailByIdTests(CrmClientFixture crmClientFixture)
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
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetContactDetailByIdQuery(contactId, new ColumnSet()));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WhenCalled_WithContactIdForExistingContact_ReturnsContactDetail()
    {
        // Arrange        
        var person = await _dataScope.TestData.CreatePerson();

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new GetContactDetailByIdQuery(person.ContactId, new ColumnSet()));

        // Assert
        Assert.NotNull(results);
        Assert.Equal(results.Contact.Id, person.ContactId);
    }
}
