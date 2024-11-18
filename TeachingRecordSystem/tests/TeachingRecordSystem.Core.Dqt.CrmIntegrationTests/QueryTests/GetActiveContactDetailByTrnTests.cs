using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetActiveContactDetailByTrnTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetActiveContactDetailByTrnTests(CrmClientFixture crmClientFixture)
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
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByTrnQuery(trn, new ColumnSet()));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WhenCalled_WithTrnForExistingContact_ReturnsContactDetail()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePersonAsync(p => p.WithTrn());

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByTrnQuery(person.Trn!, new ColumnSet()));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Contact.Id, person.ContactId);
        Assert.Empty(result.PreviousNames);
    }

    [Fact]
    public async Task WhenCalled_WithTrnForExistingContactWithPreviousName_ReturnsContactDetailIncludingPreviousNames()
    {
        // Arrange
        var updatedFirstName = _dataScope.TestData.GenerateFirstName();
        var updatedMiddleName = _dataScope.TestData.GenerateMiddleName();
        var updatedLastName = _dataScope.TestData.GenerateLastName();
        var person = await _dataScope.TestData.CreatePersonAsync(p => p.WithTrn());
        await _dataScope.TestData.UpdatePersonAsync(b => b.WithPersonId(person.ContactId).WithUpdatedName(updatedFirstName, updatedMiddleName, updatedLastName));

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByTrnQuery(person.Trn!, new ColumnSet()));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Contact.Id, person.ContactId);
        Assert.Equal(3, result.PreviousNames.Length);
    }
}
