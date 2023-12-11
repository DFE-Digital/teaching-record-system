namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetAllSpecialismsTests
{
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetAllSpecialismsTests(CrmClientFixture crmClientFixture)
    {
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var query = new GetAllSpecialismsQuery();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        Assert.NotEmpty(result);
    }
}
