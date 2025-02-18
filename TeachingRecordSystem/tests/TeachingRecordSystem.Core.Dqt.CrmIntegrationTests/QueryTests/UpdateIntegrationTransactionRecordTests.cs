// namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;
//
// public class UpdateIntegrationTransactionRecordTests : IAsyncLifetime
// {
//     private readonly CrmClientFixture.TestDataScope _dataScope;
//     private readonly CrmQueryDispatcher _crmQueryDispatcher;
//
//     public UpdateIntegrationTransactionRecordTests(CrmClientFixture crmClientFixture)
//     {
//         _dataScope = crmClientFixture.CreateTestDataScope();
//         _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
//     }
//
//     public Task InitializeAsync() => Task.CompletedTask;
//
//     public async Task DisposeAsync() => await _dataScope.DisposeAsync();
//
//     [Fact]
//     public async Task QueryExecutesSuccessfully()
//     {
//         // Arrange
//         var activeNpqQualificationType = dfeta_qualification_dfeta_Type.NPQLT;
//         var activeNpqQualificationId = Guid.NewGuid();
//         var startDate = new DateTime(2011, 01, 1);
//         var typeId = dfeta_IntegrationInterface.GTCWalesImport;
//         var reference = "1";
//         var rowData = "SOME ROW DATA";
//         var statusCode = dfeta_integrationtransactionrecord_StatusCode.Fail;
//         var failureMessage = "THIS IS A FAILURE MESSAGE";
//         var fileName = "QTS_FAILEDFILE.csv";
//         Guid? itrId = null;
//
//         var establishment1 = await _dataScope.TestData.CreateAccountAsync(x =>
//         {
//             x.WithName("SomeAccountName");
//         });
//         var person = await _dataScope.TestData.CreatePersonAsync(b => b
//             .WithQts(new DateOnly(2021, 01, 1))
//             .WithInduction(inductionStatus: dfeta_InductionStatus.Pass, inductionExemptionReason: null, inductionPeriodStartDate: new DateOnly(2021, 01, 01), completedDate: new DateOnly(2022, 01, 01), inductionStartDate: new DateOnly(2021, 01, 01), inductionPeriodEndDate: new DateOnly(2022, 01, 01), appropriateBodyOrgId: establishment1.AccountId)
//             .WithQualification(activeNpqQualificationId, activeNpqQualificationType, isActive: true));
//
//         var query = new CreateIntegrationTransactionQuery()
//         {
//             TypeId = (int)typeId,
//             StartDate = startDate,
//             FileName = "FILENAME.csv"
//         };
//         var integrationTransactionId = await _crmQueryDispatcher.ExecuteQueryAsync(query);
//
//         var recordQuery = new CreateIntegrationTransactionRecordTransactionalQuery()
//         {
//             IntegrationTransactionId = integrationTransactionId,
//             Reference = reference,
//             ContactId = person.PersonId,
//             InitialTeacherTrainingId = null,
//             QualificationId = null,
//             InductionId = null,
//             InductionPeriodId = null,
//             DuplicateStatus = dfeta_integrationtransactionrecord_dfeta_DuplicateStatus.Duplicate,
//             FileName = fileName,
//             FailureMessage = "",
//             StatusCode = dfeta_integrationtransactionrecord_StatusCode.Fail,
//             RowData = "",
//         };
//         using var txn = _crmQueryDispatcher.CreateTransactionRequestBuilder();
//         var itr = txn.AppendQuery(recordQuery);
//         await txn.ExecuteAsync();
//         itrId = itr();
//
//
//         using var txn2 = _crmQueryDispatcher.CreateTransactionRequestBuilder();
//         var updateRecordQuery = new UpdateIntegrationTransactionRecordTransactionalQuery()
//         {
//             IntegrationTransactionRecordId = itrId.Value,
//             IntegrationTransactionId = integrationTransactionId,
//             Reference = reference,
//             PersonId = person.PersonId,
//             InitialTeacherTrainingId = null,
//             QualificationId = null,
//             InductionId = person.DqtInductions.First().InductionId,
//             InductionPeriodId = person.DqtInductionPeriods.First().InductionPeriodId,
//             DuplicateStatus = dfeta_integrationtransactionrecord_dfeta_DuplicateStatus.Duplicate,
//             FailureMessage = failureMessage,
//             StatusCode = statusCode,
//             RowData = rowData,
//         };
//         txn2.AppendQuery(updateRecordQuery);
//
//         // Act
//         await txn2.ExecuteAsync();
//
//         // Assert
//         using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
//         var createdIntegrationTransaction = ctx.dfeta_integrationtransactionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
//         var updatedIntegrationTransactionRecord = ctx.dfeta_integrationtransactionrecordSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.PrimaryIdAttribute) == itrId);
//         Assert.NotNull(createdIntegrationTransaction);
//         Assert.NotNull(updatedIntegrationTransactionRecord);
//         Assert.Equal(integrationTransactionId, updatedIntegrationTransactionRecord.dfeta_IntegrationTransactionId.Id);
//         Assert.Equal(reference, updatedIntegrationTransactionRecord.dfeta_id);
//         Assert.Equal(person.ContactId, updatedIntegrationTransactionRecord.dfeta_PersonId.Id);
//         Assert.Equal(person.DqtInductions.First().InductionId, updatedIntegrationTransactionRecord.dfeta_InductionId.Id);
//         Assert.Equal(person.DqtInductionPeriods.First().InductionPeriodId, updatedIntegrationTransactionRecord.dfeta_InductionPeriodId.Id);
//         Assert.Equal(dfeta_integrationtransactionrecord_dfeta_DuplicateStatus.Duplicate, updatedIntegrationTransactionRecord.dfeta_DuplicateStatus);
//         Assert.Equal(rowData, updatedIntegrationTransactionRecord.dfeta_RowData);
//         Assert.Equal(failureMessage, updatedIntegrationTransactionRecord.dfeta_FailureMessage);
//         Assert.Equal(statusCode, updatedIntegrationTransactionRecord.StatusCode);
//         Assert.Equal(fileName, updatedIntegrationTransactionRecord.dfeta_Filename);
//     }
// }
