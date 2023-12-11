namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetAllTeacherStatusesTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllTeacherStatusesTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllTeacherStatusesQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        Assert.NotEmpty(result);
    }
}
