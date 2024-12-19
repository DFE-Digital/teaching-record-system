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
        using var txn = _crmQueryDispatcher.CreateTransactionRequestBuilder();
        var contact = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithTrn();
            x.WithQts();
        });
        var qtsDate = new DateTime(2024, 01, 01);
        var teacherStatusQualifiedTeacherTrained = "213";
        var query = new GetAllTeacherStatusesQuery();
        var result = await _crmQueryDispatcher.ExecuteQueryAsync(query);
        var teacherStatusId = result.FirstOrDefault(x => x.dfeta_Value == teacherStatusQualifiedTeacherTrained);
        var qtsId = Guid.NewGuid();
        var queryCreateQts = new CreateQtsRegistrationTransactionalQuery()
        {
            Id = qtsId,
            ContactId = contact.PersonId,
            TeacherStatusId = teacherStatusId!.Id,
            QtsDate = qtsDate
        };
        txn.AppendQuery(queryCreateQts);

        // Act
        await txn.ExecuteAsync();

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var qts = ctx.dfeta_qtsregistrationSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_qtsregistration.PrimaryIdAttribute) == qtsId);
        Assert.NotNull(qts);
        Assert.Equal(qtsDate, qts.dfeta_QTSDate);
        Assert.Equal(contact.PersonId, qts.dfeta_PersonId.Id);
    }
}
