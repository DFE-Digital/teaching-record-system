using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetContactByTrnTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetContactByTrnTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_WithTrnForNonExistentContact_ReturnsNull()
    {
        // Arrange        
        var trn = "DodgyTrn";

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetContactByTrnQuery(trn, new ColumnSet()));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WhenCalled_WithTrnForExistingContact_ReturnsContactDetail()
    {
        // Arrange        
        var person = await _dataScope.TestData.CreatePerson(b => b.WithTrn());

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetContactByTrnQuery(person.Trn!, new ColumnSet()));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Id, person.ContactId);
    }
}
