using System.Globalization;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class EwcWalesImportJobTests : IClassFixture<EwcWalesImportJobFixture>
{
    public EwcWalesImportJobTests(EwcWalesImportJobFixture fixture)
    {
        Fixture = fixture;
    }

    private DbFixture DbFixture => Fixture.DbFixture;

    private IClock Clock => Fixture.Clock;

    private TestData TestData => Fixture.TestData;

    private EwcWalesImportJobFixture Fixture { get; }

    public IOrganizationServiceAsync2 OrganizationService => Fixture.OrganizationService;

    private EwcWalesImportJob Job => Fixture.Job;

    [Theory]
    [InlineData("IND", EwcWalesImportFileType.Induction)]
    [InlineData("QTS", EwcWalesImportFileType.Qualification)]
    [InlineData("", null)]
    public void EwcWalesImportJob_GetImportFileType_ReturnsExpected(string filename, EwcWalesImportFileType? importType)
    {
        Fixture.Job.TryGetImportFileType(filename, out var type);
        Assert.Equal(importType, type);
    }

    [Fact]
    public async Task EwcWalesImportJobQts_ImportsQtsFileSuccessfully()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var qualification = ctx.dfeta_qualificationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qualification.Fields.dfeta_PersonId) == person.ContactId);
        var itt = ctx.dfeta_initialteachertrainingSet.Single(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person.ContactId);
        var qts = ctx.dfeta_qtsregistrationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person.ContactId);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords, item1 =>
        {
            Assert.Empty(item1.dfeta_FailureMessage);
        });
        Assert.NotNull(qualification);
        Assert.NotNull(itt);
        Assert.NotNull(qts);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
        Assert.Equal(fileName, integrationTransaction.dfeta_Filename);
    }

    [Fact]
    public async Task EwcWalesImportJobQts_WithActiveAlert_ImportsQtsFileSuccessfully()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithTrn();
            x.WithAlert();
        });
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedTaskCategory = "GTC Wales Import";
        var expectedTaskDescription = "QTS/Induction update with Active Sanction";
        var expectedTaskSubject = "Notification for QTS Unit Team";

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var qualification = ctx.dfeta_qualificationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qualification.Fields.dfeta_PersonId) == person.ContactId);
        var itt = ctx.dfeta_initialteachertrainingSet.Single(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person.ContactId);
        var qts = ctx.dfeta_qtsregistrationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person.ContactId);
        var task = ctx.TaskSet.Single(i => i.GetAttributeValue<Guid>(Dqt.Models.Task.Fields.RegardingObjectId) == person.PersonId);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords, item1 =>
        {
            Assert.Empty(item1.dfeta_FailureMessage);
        });
        Assert.NotNull(qualification);
        Assert.NotNull(itt);
        Assert.NotNull(qts);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
        Assert.Equal(fileName, integrationTransaction.dfeta_Filename);
        Assert.NotNull(task);
        Assert.Equal(expectedTaskDescription, task.Description);
        Assert.Equal(expectedTaskSubject, task.Subject);
        Assert.Equal(expectedTaskCategory, task.Category);
    }


    [Fact]
    public async Task EwcWalesImportJobQts_SingleSuccessAndFailure_ReturnsExpectedCounts()
    {
        // Arrange
        var expectedTotalRowCount = 2;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var expectedValueMessage = $"Teacher with TRN {trn} has QTS already.";
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var qualification = ctx.dfeta_qualificationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qualification.Fields.dfeta_PersonId) == person.ContactId);
        var itt = ctx.dfeta_initialteachertrainingSet.Single(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person.ContactId);
        var qts = ctx.dfeta_qtsregistrationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person.ContactId);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Empty(item1.dfeta_FailureMessage);
            },
            item2 =>
            {
                Assert.Contains(expectedValueMessage, item2.dfeta_FailureMessage);
            });
        Assert.NotNull(qualification);
        Assert.NotNull(itt);
        Assert.NotNull(qts);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.NotEmpty(integrationTransaction.dfeta_FailureMessage);
        Assert.Contains(expectedValueMessage, integrationTransaction.dfeta_FailureMessage, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task EwcWalesImportJobQts_MultipleSuccessMultipleFailure_ReturnsExpectedCounts()
    {
        // Arrange
        var expectedTotalRowCount = 4;
        var expectedSuccessCount = 2;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 2;
        var person1 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person1.Trn;
        var person2 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn2 = person2.Trn;
        var expectedValueMessage1 = $"Teacher with TRN {trn1} has QTS already.";
        var expectedValueMessage2 = $"Teacher with TRN {trn2} has QTS already.";
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn1},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{trn1},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{trn2},firstname,lastname,{person2.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{trn2},firstname,lastname,{person2.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var qualificationPerson1 = ctx.dfeta_qualificationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qualification.Fields.dfeta_PersonId) == person1.ContactId);
        var ittPerson1 = ctx.dfeta_initialteachertrainingSet.Single(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person1.ContactId);
        var qtsPerson1 = ctx.dfeta_qtsregistrationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person1.ContactId);
        var qualificationPerson2 = ctx.dfeta_qualificationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qualification.Fields.dfeta_PersonId) == person2.ContactId);
        var ittPerson2 = ctx.dfeta_initialteachertrainingSet.Single(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person2.ContactId);
        var qtsPerson2 = ctx.dfeta_qtsregistrationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person2.ContactId);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Empty(item1.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item1.StatusCode);
            },
            item2 =>
            {
                Assert.Contains(expectedValueMessage1, item2.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Fail, item2.StatusCode);

            },
            item3 =>
            {
                Assert.Empty(item3.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item3.StatusCode);
            },
            item4 =>
            {
                Assert.Contains(expectedValueMessage2, item4.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Fail, item4.StatusCode);
            });
        Assert.NotNull(qualificationPerson1);
        Assert.NotNull(ittPerson1);
        Assert.NotNull(qtsPerson1);
        Assert.NotNull(qualificationPerson2);
        Assert.NotNull(ittPerson2);
        Assert.NotNull(qtsPerson2);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.NotEmpty(integrationTransaction.dfeta_FailureMessage);
        Assert.Contains(expectedValueMessage1, integrationTransaction.dfeta_FailureMessage, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(expectedValueMessage2, integrationTransaction.dfeta_FailureMessage, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task EwcWalesImportJobQts_WithQualifiedTeacherQTSStatus_ReturnsSuccess()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var person1 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person1.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn1},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},67,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var qualificationPerson1 = ctx.dfeta_qualificationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qualification.Fields.dfeta_PersonId) == person1.ContactId);
        var ittPerson1 = ctx.dfeta_initialteachertrainingSet.Single(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.Fields.dfeta_PersonId) == person1.ContactId);
        var qtsRegistration = ctx.dfeta_qtsregistrationSet.Single(i => i.GetAttributeValue<Guid>(dfeta_qtsregistration.Fields.dfeta_PersonId) == person1.ContactId);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.Fields.dfeta_PersonId) == person1.ContactId);
        Assert.NotNull(integrationTransaction);
        Assert.NotNull(qtsRegistration);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Empty(item1.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item1.StatusCode);
            });
        Assert.NotNull(qualificationPerson1);
        Assert.NotNull(ittPerson1);
        Assert.NotNull(induction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_NoExistingInduction_CreatesInductionAndReturnsExpectedCounts()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person.Trn;
        var inductionStartDate = new DateTime(2024, 05, 01);
        var inductionPassDate = new DateTime(2024, 10, 07);
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{person.Trn!},Keri Louise Lyddon,Nicholas,{person.DateOfBirth.ToString("dd/MM/yyyy")},{inductionStartDate.ToString("dd/MM/yyyy")},{inductionPassDate.ToString("dd/MM/yyyy")},,Pembrokeshire Local Authority,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.Fields.dfeta_PersonId) == person.ContactId);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Empty(item1.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item1.StatusCode);
            });
        Assert.NotNull(induction);
        Assert.Equal(dfeta_InductionStatus.PassedinWales, induction.dfeta_InductionStatus);
        Assert.Equal(inductionStartDate, induction.dfeta_StartDate);
        Assert.Equal(inductionPassDate, induction.dfeta_CompletionDate);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_DateOfBirthDoesNotMatch_FailsReturnsExpectedCounts()
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person.Trn;
        var inductionStartDate = new DateTime(2024, 05, 01);
        var inductionPassDate = new DateTime(2024, 10, 07);
        var dob = "01/09/1977";
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{person.Trn!},Keri Louise Lyddon,Nicholas,{dob},{inductionStartDate.ToString()},{inductionPassDate.ToString()},,Pembrokeshire Local Authority,{account},Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.FirstOrDefault(i => i.GetAttributeValue<Guid>(dfeta_induction.Fields.dfeta_PersonId) == person.ContactId);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Contains($"For TRN {person.Trn!} Date of Birth does not match with the existing record.", item1.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Fail, item1.StatusCode);
            });
        Assert.Null(induction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.NotEmpty(integrationTransaction.dfeta_FailureMessage);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_ValidRow_ReturnsSuccessAndExpectedCounts()
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person.Trn;
        var inductionStartDate = new DateTime(2024, 05, 01);
        var inductionPassDate = new DateTime(2024, 10, 07);
        var fileName = "IND.csv";
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{person.Trn!},Keri Louise Lyddon,Nicholas,{person.DateOfBirth.ToString("dd/MM/yyyy")},{inductionStartDate.ToString("dd/MM/yyyy")},{inductionPassDate.ToString("dd/MM/yyyy")},,{account.AccountNumber},,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.Fields.dfeta_PersonId) == person.ContactId);
        var inductionPeriod = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_inductionperiod.Fields.dfeta_InductionId) == induction.Id);
        Assert.NotNull(integrationTransaction);
        Assert.NotNull(induction);
        Assert.NotNull(inductionPeriod);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Empty(item1.dfeta_FailureMessage);
                Assert.Equal(fileName, item1.dfeta_Filename);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item1.StatusCode);
            });
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_WithInvalidEmployerCode_ReturnsExpectedCounts()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person.Trn;
        var inductionStartDate = new DateTime(2024, 05, 01);
        var inductionPassDate = new DateTime(2024, 10, 07);
        var invalidEmployeCode = "invalid";
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{person.Trn!},Keri Louise Lyddon,Nicholas,{person.DateOfBirth.ToString("dd/MM/yyyy")},{inductionStartDate.ToString("dd/MM/yyyy")},{inductionPassDate.ToString("dd/MM/yyyy")},,Pembrokeshire Local Authority,{invalidEmployeCode},Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.Fields.dfeta_PersonId) == person.ContactId);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Contains($"Organisation with Induction Body Code {invalidEmployeCode} was not found.", item1.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item1.StatusCode);
            });
        Assert.NotNull(induction);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.NotEmpty(integrationTransaction.dfeta_FailureMessage);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_WithInvalidTRN_ReturnsExpectedCounts()
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var inductionStartDate = new DateTime(2024, 05, 01);
        var inductionPassDate = new DateTime(2024, 10, 07);
        var invalidTRN = "invalid";
        var expectedFailureMessage = $"Teacher with TRN {invalidTRN} was not found.";
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{invalidTRN},Keri Louise Lyddon,Nicholas,01/01/2024,{inductionStartDate.ToString()},{inductionPassDate.ToString()},,{account.AccountNumber},12345,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Contains(expectedFailureMessage, item1.dfeta_FailureMessage);
                Assert.Null(item1.dfeta_PersonId);
                Assert.Null(item1.dfeta_InductionPeriodId);
                Assert.Null(item1.dfeta_InductionId);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Fail, item1.StatusCode);
            });
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.NotEmpty(integrationTransaction.dfeta_FailureMessage);
    }

    [Fact]
    public async Task EwcWalesImportJob_ImportsInvalidFileType_ReturnsExpected()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},49, 04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("INVALID_FILETYPE.csv", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        Assert.Null(integrationTransactionId);
        Assert.Null(integrationTransaction);
        Fixture.Logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Import filename must begin with IND or QTS")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_ImportsInductionFileSuccessfully()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var accountNumber = "9999";
        var startDate = DateTime.ParseExact("17/09/2014", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var passDate = DateTime.ParseExact("28/09/2017", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{startDate.ToString("dd/MM/yyyy")},{passDate.ToString("dd/MM/yyyy")},,{account.Name},{accountNumber},Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        //Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecord = ctx.dfeta_integrationtransactionrecordSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == itrRecord.dfeta_InductionId.Id);
        var inductionPeriod = ctx.dfeta_inductionperiodSet.Single(i => i.GetAttributeValue<Guid>(dfeta_inductionperiod.PrimaryIdAttribute) == itrRecord.dfeta_InductionPeriodId.Id);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
        Assert.Equal(startDate, induction.dfeta_StartDate);
        Assert.Equal(passDate, induction.dfeta_CompletionDate);
        Assert.Equal(startDate, inductionPeriod.dfeta_StartDate);
        Assert.Equal(passDate, inductionPeriod.dfeta_EndDate);
        Assert.Equal(account.Id, inductionPeriod.dfeta_AppropriateBodyId.Id);
    }

    [Theory]
    [InlineData(dfeta_InductionStatus.InProgress, null)]
    [InlineData(dfeta_InductionStatus.Pass, null)]
    [InlineData(dfeta_InductionStatus.PassedinWales, null)]
    [InlineData(dfeta_InductionStatus.Exempt, dfeta_InductionExemptionReason.Exempt)]
    [InlineData(dfeta_InductionStatus.Fail, null)]
    [InlineData(dfeta_InductionStatus.FailedinWales, null)]
    public async Task EwcWalesImportJobInduction_WithInductionstatusThatCannotBeUpdated_ReturnsError(dfeta_InductionStatus status, dfeta_InductionExemptionReason? exemptionReason)
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var accountNumber = "54321";
        var startDate = DateTime.ParseExact("17/09/2014", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var passDate = DateTime.ParseExact("28/09/2017", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithTrn();
            x.WithQts();
            x.WithDqtInduction(status, exemptionReason, new DateOnly(1999, 01, 01), null);
        });
        var expectedValueMessage = $"Teacher with TRN {person.Trn} completed induction already or is progress.";
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{startDate.ToString("dd/MM/yyyy")},{passDate.ToString("dd/MM/yyyy")},,{account.Name},{accountNumber},Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        //Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecord = ctx.dfeta_integrationtransactionrecordSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == itrRecord.dfeta_InductionId.Id);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Contains(expectedValueMessage, integrationTransaction.dfeta_FailureMessage, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_InductionPassedDateBeforeQtsDate_ReturnsError()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var accountNumber = "54321";
        var startDate = DateTime.ParseExact("17/09/2000", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var passDate = DateTime.ParseExact("28/09/1991", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithTrn();
            x.WithQts(new DateOnly(1999, 04, 05));
        });
        var expectedValueMessage = $"Induction passed date cannot be before Qts Date.";
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{startDate.ToString("dd/MM/yyyy")},{passDate.ToString("dd/MM/yyyy")},,{account.Name},{accountNumber},Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        //Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecord = ctx.dfeta_integrationtransactionrecordSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Contains(expectedValueMessage, integrationTransaction.dfeta_FailureMessage, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(expectedValueMessage, itrRecord.dfeta_FailureMessage);
    }

    [Fact]
    public async Task EwcWalesImportJobInductionWithoutAppropriateBody_ImportsInductionFileSuccessfully()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var startDate = DateTime.ParseExact("17/09/2014", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var passDate = DateTime.ParseExact("28/09/2017", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{startDate.ToString("dd/MM/yyyy")},{passDate.ToString("dd/MM/yyyy")},,,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        //Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecord = ctx.dfeta_integrationtransactionrecordSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == itrRecord.dfeta_InductionId.Id);
        var inductionPeriod = ctx.dfeta_inductionperiodSet.Single(i => i.GetAttributeValue<Guid>(dfeta_inductionperiod.PrimaryIdAttribute) == itrRecord.dfeta_InductionPeriodId.Id);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
        Assert.Equal(startDate, induction.dfeta_StartDate);
        Assert.Equal(passDate, induction.dfeta_CompletionDate);
        Assert.Equal(startDate, inductionPeriod.dfeta_StartDate);
        Assert.Equal(passDate, inductionPeriod.dfeta_EndDate);
        Assert.Null(inductionPeriod.dfeta_AppropriateBodyId);
    }

    [Fact]
    public async Task EwcWalesImportJobInductionWithActiveAlert_ImportsInductionFileSuccessfully()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var startDate = DateTime.ParseExact("17/09/2014", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var passDate = DateTime.ParseExact("28/09/2017", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithTrn();
            x.WithAlert();
        });
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{startDate.ToString("dd/MM/yyyy")},{passDate.ToString("dd/MM/yyyy")},,,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);
        var expectedTaskCategory = "GTC Wales Import";
        var expectedTaskDescription = "QTS/Induction update with Active Sanction";
        var expectedTaskSubject = "Notification for QTS Unit Team";

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        //Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecord = ctx.dfeta_integrationtransactionrecordSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == itrRecord.dfeta_InductionId.Id);
        var inductionPeriod = ctx.dfeta_inductionperiodSet.Single(i => i.GetAttributeValue<Guid>(dfeta_inductionperiod.PrimaryIdAttribute) == itrRecord.dfeta_InductionPeriodId.Id);
        var task = ctx.TaskSet.Single(i => i.GetAttributeValue<Guid>(Dqt.Models.Task.Fields.RegardingObjectId) == person.PersonId);

        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
        Assert.Equal(startDate, induction.dfeta_StartDate);
        Assert.Equal(passDate, induction.dfeta_CompletionDate);
        Assert.Equal(startDate, inductionPeriod.dfeta_StartDate);
        Assert.Equal(passDate, inductionPeriod.dfeta_EndDate);
        Assert.Null(inductionPeriod.dfeta_AppropriateBodyId);
        Assert.NotNull(task);
        Assert.Equal(expectedTaskDescription, task.Description);
        Assert.Equal(expectedTaskSubject, task.Subject);
        Assert.Equal(expectedTaskCategory, task.Category);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_UpdatesExistingInduction()
    {
        // Arrange
        var accountNumber = "678910";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var startDate = DateTime.ParseExact("17/09/2014", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var passDate = DateTime.ParseExact("28/09/2017", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithTrn();
            x.WithDqtInduction(inductionStatus: dfeta_InductionStatus.RequiredtoComplete,
                inductionExemptionReason: null,
                startDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                completedDate: null,
                inductionPeriodStartDate: startDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                inductionPeriodEndDate: null,
                appropriateBodyOrgId: account.Id);
        });
        var trn = person.Trn;
        var updatedStartDate = DateTime.ParseExact("17/09/2019", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var updatedPassDate = DateTime.ParseExact("28/09/2020", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{updatedStartDate.ToString("dd/MM/yyyy")},{updatedPassDate.ToString("dd/MM/yyyy")},,,{accountNumber},Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        //Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecord = ctx.dfeta_integrationtransactionrecordSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var induction = ctx.dfeta_inductionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == itrRecord.dfeta_InductionId.Id);
        var inductionPeriod = ctx.dfeta_inductionperiodSet.Single(i => i.GetAttributeValue<Guid>(dfeta_inductionperiod.PrimaryIdAttribute) == itrRecord.dfeta_InductionPeriodId.Id);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
        Assert.Equal(updatedPassDate, induction.dfeta_CompletionDate);
        Assert.Equal(updatedPassDate, inductionPeriod.dfeta_EndDate);
        Assert.Equal(account.Id, inductionPeriod.dfeta_AppropriateBodyId.Id);
    }
}

public class EwcWalesImportJobFixture : IAsyncLifetime
{
    public EwcWalesImportJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        IServiceProvider provider,
        ILoggerFactory loggerFactory)
    {
        OrganizationService = provider.GetService<IOrganizationServiceAsync2>()!;
        DbFixture = dbFixture;
        Clock = new();
        Helper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            OrganizationService,
            referenceDataCache,
            Clock,
            new TestableAuditRepository(),
            loggerFactory.CreateLogger<TrsDataSyncHelper>());

        var blobServiceClient = new Mock<BlobServiceClient>();
        var qtsImporter = ActivatorUtilities.CreateInstance<QtsImporter>(provider);
        var inductionImporter = ActivatorUtilities.CreateInstance<InductionImporter>(provider);
        Logger = new Mock<ILogger<EwcWalesImportJob>>();
        Job = ActivatorUtilities.CreateInstance<EwcWalesImportJob>(provider, blobServiceClient.Object, qtsImporter, inductionImporter, Logger.Object);
        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            OrganizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));
    }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }

    public Mock<ILogger<EwcWalesImportJob>> Logger { get; }

    Task IAsyncLifetime.InitializeAsync() => DbFixture.WithDbContextAsync(dbContext => dbContext.Events.ExecuteDeleteAsync());

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public IOrganizationServiceAsync2 OrganizationService { get; }

    public EwcWalesImportJob Job { get; }
}
