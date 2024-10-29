namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateIntegrationTransactionRecordTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateIntegrationTransactionRecordTests(CrmClientFixture crmClientFixture)
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
        var activeNpqQualificationType = dfeta_qualification_dfeta_Type.NPQLT;
        var activeNpqQualificationId = Guid.NewGuid();
        var startDate = new DateTime(2011, 01, 1);
        var typeId = dfeta_IntegrationInterface.GTCWalesImport;
        var reference = "1";

        var establishment1 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName("SomeAccountName");
        });
        var person = await _dataScope.TestData.CreatePerson(b => b
            .WithQts(new DateOnly(2021, 01, 1))
            .WithInduction(inductionStatus: dfeta_InductionStatus.Pass, inductionExemptionReason: null, inductionPeriodStartDate: new DateOnly(2021, 01, 01), completedDate: new DateOnly(2022, 01, 01), inductionStartDate: new DateOnly(2021, 01, 01), inductionPeriodEndDate: new DateOnly(2022, 01, 01), appropriateBodyOrgId: establishment1.AccountId)
            .WithQualification(activeNpqQualificationId, activeNpqQualificationType, isActive: true));

        var query = new CreateIntegrationTransactionQuery()
        {
            TypeId = (int)typeId,
            StartDate = startDate
        };
        var integrationTransactionId = await _crmQueryDispatcher.ExecuteQuery(query);

        var recordQuery = new CreateIntegrationTransactionRecordQuery()
        {
            IntegrationTransactionId = integrationTransactionId,
            Reference = reference,
            PersonId = person.PersonId,
            InitialTeacherTrainingId = null,
            QualificationId = null,
            InductionId = null,
            InductionPeriodId = null,
            Duplicate = true
        };
        var integrationTransactionRecordId = await _crmQueryDispatcher.ExecuteQuery(recordQuery);
        var updateRecordQuery = new UpdateIntegrationTransactionRecordQuery()
        {
            IntegrationTransactionRecordId = integrationTransactionRecordId,
            IntegrationTransactionId = integrationTransactionId,
            Reference = reference,
            PersonId = person.PersonId,
            InitialTeacherTrainingId = null,
            QualificationId = null,
            InductionId = person.Inductions.First().InductionId,
            InductionPeriodId = person.InductionPeriods.First().InductionPeriodId,
            Duplicate = true
        };

        // Act
        var updated = await _crmQueryDispatcher.ExecuteQuery(updateRecordQuery);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var createdIntegrationTransaction = ctx.dfeta_integrationtransactionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var updatedIntegrationTransaction = ctx.dfeta_integrationtransactionrecordSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.PrimaryIdAttribute) == integrationTransactionRecordId);
        Assert.NotNull(createdIntegrationTransaction);
        Assert.NotNull(updatedIntegrationTransaction);
        Assert.Equal(updatedIntegrationTransaction.dfeta_IntegrationTransactionId.Id, integrationTransactionId);
        Assert.Equal(updatedIntegrationTransaction.dfeta_id, reference);
        Assert.Equal(updatedIntegrationTransaction.dfeta_PersonId.Id, person.ContactId);
        Assert.Equal(updatedIntegrationTransaction.dfeta_InductionId.Id, person.Inductions.First().InductionId);
        Assert.Equal(updatedIntegrationTransaction.dfeta_InductionPeriodId.Id, person.InductionPeriods.First().InductionPeriodId);
        Assert.Equal(updatedIntegrationTransaction.dfeta_DuplicateStatus, dfeta_integrationtransactionrecord_dfeta_DuplicateStatus.Duplicate);
    }
}
