namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateQTSTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CreateQTSTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var contact = await _dataScope.TestData.CreatePerson();
        var qtsDate = new DateTime(2024, 01, 01);
        var teacherStatusQualifiedTeacherTrained = "211";
        var query = new GetAllTeacherStatusesQuery();
        var result = await _crmQueryDispatcher.ExecuteQuery(query);
        var teacherStatusId = result.FirstOrDefault(x => x.dfeta_Value == teacherStatusQualifiedTeacherTrained);
        var queryCreateQts = new CreateQTSQuery()
        {
            PersonId = contact.PersonId,
            TeacherStatusId = teacherStatusId!.Id,
            QTSDate = null
        };

        // Act
        var createQtsId = await _crmQueryDispatcher.ExecuteQuery(queryCreateQts);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var qts = ctx.dfeta_qtsregistrationSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_qtsregistration.PrimaryIdAttribute) == createQtsId);
        Assert.NotNull(qts);
        Assert.Equal(qts.Id, createQtsId);
        Assert.Null(qts.dfeta_QTSDate);
        Assert.Equal(qts.dfeta_PersonId.Id, contact.PersonId);
    }

    //Test that inserts with a status that allows qtsdate to be set.
}
