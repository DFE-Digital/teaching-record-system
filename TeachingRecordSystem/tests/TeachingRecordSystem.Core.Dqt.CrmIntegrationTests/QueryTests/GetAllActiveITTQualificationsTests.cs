namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetAllActiveITTQualificationsTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllActiveITTQualificationsTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllActiveITTQualificationsQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.NotEmpty(result);
    }
}
