using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetContactByTrnRequestIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetContactByTrnRequestIdTests(CrmClientFixture crmClientFixture)
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
        var requestId = Guid.NewGuid().ToString();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(new GetContactByTrnRequestIdQuery(requestId, new ColumnSet()));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WhenCalled_WithTrnForExistingContact_ReturnsContactDetail()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var requestId = Guid.NewGuid().ToString();
        var person = await _dataScope.TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithTrnRequest(applicationUserId, requestId));

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(
            new GetContactByTrnRequestIdQuery(TrnRequestHelper.GetCrmTrnRequestId(applicationUserId, requestId), new ColumnSet()));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.Id, person.ContactId);
    }
}
