namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetAllActiveIttQualificationsTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllActiveIttQualificationsTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllActiveIttQualificationsQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.NotEmpty(result);
    }
}
