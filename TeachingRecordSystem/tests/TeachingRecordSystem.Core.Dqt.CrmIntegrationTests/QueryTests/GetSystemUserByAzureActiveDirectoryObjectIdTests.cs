namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetSystemUserByAzureActiveDirectoryObjectIdTests : IAsyncLifetime
{
    private const string KnownCrmAzureActiveDirectoryObjectId = "4F52E663-AA5F-4D1B-A9A7-449E73481BB4"; // This is the Azure AD object ID of the "Teaching Record System (dev)" user in the CRM build environment
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetSystemUserByAzureActiveDirectoryObjectIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_WithAzureActiveDirectoryObjectIdForNonExistentUser_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var query = new GetSystemUserByAzureActiveDirectoryObjectIdQuery(nonExistentId);

        // Act
        var systemUserInfo = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.Null(systemUserInfo);
    }

    [Fact]
    public async Task WhenCalled_WithAzureActiveDirectoryObjectIdForExistingUser_ReturnsSystemUserInfo()
    {
        // Arrange        
        var query = new GetSystemUserByAzureActiveDirectoryObjectIdQuery(KnownCrmAzureActiveDirectoryObjectId);

        // Act
        var systemUserInfo = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.NotNull(systemUserInfo);
        Assert.Equal(false, systemUserInfo.SystemUser.IsDisabled);
        Assert.NotEmpty(systemUserInfo.Roles);
    }
}
