using System;
using System.Globalization;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using TeachingRecordSystem.Core.Services.Files;
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

    private MessageSerializer MessageSerializer => Fixture.MessageSerializer;


    [Theory]
    [InlineData("IND", EwcWalesImportFileType.Induction)]
    [InlineData("QTS", EwcWalesImportFileType.Qualification)]
    [InlineData("", null)]
    public void EwcWalesImportJob_GetImportFileType_ReturnsExpected(string filename, EwcWalesImportFileType? importType)
    {
        Fixture.Job.TryGetImportFileType(filename, out var type);
        Assert.Equal(importType, type);
    }

    [Theory]
    [InlineData("", "Qts status is missing")]
    [InlineData("3343", "Qts Status must be 49,67, 68,69, 71 or 102")]
    public async Task EwcWalesImportJobQts_WithInvalidQtsStatus_ReturnsError(string qtsStatus, string expectedErrorMessage)
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},{qtsStatus},04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords, item1 =>
        {
            Assert.Contains(expectedErrorMessage, item1.dfeta_FailureMessage);
        });
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Contains(expectedErrorMessage, integrationTransaction.dfeta_FailureMessage);
        Assert.Equal(fileName, integrationTransaction.dfeta_Filename);
    }

    [Theory]
    [InlineData("67")]
    [InlineData("71")]
    [InlineData("49")]
    public async Task EwcWalesImportJobQts_Format1ImportsQtsFileSuccessfully(string qtsStatus)
    {
        // Arrange
        var awardedDate = new DateOnly(2014, 01, 04);
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},{qtsStatus},{awardedDate.ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var setInductionOutboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(SetInductionRequiredToCompleteMessage))
            .Select(x => Assert.IsType<SetInductionRequiredToCompleteMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();

        var setInductionOutboxMessage = setInductionOutboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.Equal(person.PersonId, setInductionOutboxMessage.PersonId);

        var welshROutboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddWelshRMessage))
            .Select(x => Assert.IsType<AddWelshRMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var addWelshROutboxMessage = welshROutboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.Equal(person.PersonId, addWelshROutboxMessage.PersonId);
        Assert.Equal(awardedDate.ToString(QtsImporter.DATE_FORMAT), addWelshROutboxMessage.AwardedDate?.ToString(QtsImporter.DATE_FORMAT));
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords, item1 =>
        {
            Assert.Empty(item1.dfeta_FailureMessage);
        });
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
        Assert.Equal(fileName, integrationTransaction.dfeta_Filename);
    }

    [Theory]
    [InlineData("67")]
    [InlineData("71")]
    [InlineData("49")]
    public async Task EwcWalesImportJobQts_Format2ImportsQtsFileSuccessfully(string qtsStatus)
    {
        // Arrange
        var awardedDate = new DateOnly(2014, 01, 02);
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,CODE,QTS_DATE\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},{qtsStatus},{awardedDate.ToString(QtsImporter.DATE_FORMAT)}\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        var setInductionOutboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(SetInductionRequiredToCompleteMessage))
            .Select(x => Assert.IsType<SetInductionRequiredToCompleteMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var setInductionOutboxMessage = setInductionOutboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.Equal(person.PersonId, setInductionOutboxMessage.PersonId);

        var welshROutboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddWelshRMessage))
            .Select(x => Assert.IsType<AddWelshRMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var addWelshROutboxMessage = welshROutboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.Equal(person.PersonId, addWelshROutboxMessage.PersonId);
        Assert.Equal(awardedDate, addWelshROutboxMessage.AwardedDate);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords, item1 =>
        {
            Assert.Empty(item1.dfeta_FailureMessage);
        });
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

        var task = ctx.TaskSet.Single(i => i.GetAttributeValue<Guid>(Dqt.Models.Task.Fields.RegardingObjectId) == person.PersonId);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords, item1 =>
        {
            Assert.Empty(item1.dfeta_FailureMessage);
        });
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
        var person1AwardedDate = new DateOnly(2011, 01, 04);
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.RouteToProfessionalStatusId == RouteToProfessionalStatus.WelshRId)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.DegreeTypeRequired == FieldRequirement.Optional)
            .RandomOne()
            .Value;
        var person1 = await TestData.CreatePersonAsync(p => p.WithTrn()
            .WithQts()
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Approved).WithAwardedDate(person1AwardedDate)));
        var person2 = await TestData.CreatePersonAsync(p => p.WithTrn());
        var expectedValueMessage = $"Teacher with TRN {person1.Trn} has QTS already.";
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{person1.Trn},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{person2.Trn},firstname,lastname,{person2.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item2 =>
            {
                Assert.Contains(expectedValueMessage, item2.dfeta_FailureMessage);
            },
            item1 =>
            {
                Assert.Empty(item1.dfeta_FailureMessage);
            });

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
        var person1AwardedDate = new DateOnly(2011, 01, 04);
        var person2AwardedDate = new DateOnly(2000, 01, 07);
        var route = (await TestData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.RouteToProfessionalStatusId == RouteToProfessionalStatus.WelshRId)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.DegreeTypeRequired == FieldRequirement.Optional)
            .RandomOne()
            .Value;
        var person1 = await TestData.CreatePersonAsync(p => p.WithTrn().WithQts()
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Approved).WithAwardedDate(person1AwardedDate)));
        var person2 = await TestData.CreatePersonAsync(p => p.WithTrn().WithQts()
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Approved).WithAwardedDate(person2AwardedDate)));
        var person3 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var person4 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var expectedTotalRowCount = 4;
        var expectedSuccessCount = 2;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 2;
        var expectedValueMessage1 = $"Teacher with TRN {person1.Trn} has QTS already.";
        var expectedValueMessage2 = $"Teacher with TRN {person2.Trn} has QTS already.";
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{person1.Trn},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{person2.Trn},firstname,lastname,{person2.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{person3.Trn},firstname,lastname,{person3.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{person4.Trn},firstname,lastname,{person4.DateOfBirth.ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
          item1 =>
          {
              Assert.Contains(expectedValueMessage1, item1.dfeta_FailureMessage);
              Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Fail, item1.StatusCode);

          },
         item2 =>
          {
              Assert.Contains(expectedValueMessage2, item2.dfeta_FailureMessage);
              Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Fail, item2.StatusCode);
          },
         item3 =>
         {
             Assert.Empty(item3.dfeta_FailureMessage);
             Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item3.StatusCode);
         },
         item4 =>
         {
             Assert.Empty(item4.dfeta_FailureMessage);
             Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item4.StatusCode);
         });

        var welshROutboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddWelshRMessage))
            .Select(x => Assert.IsType<AddWelshRMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var addWelshROutboxMessage1 = welshROutboxMessages.FirstOrDefault(x => x.PersonId == person1.PersonId);
        var addWelshROutboxMessage2 = welshROutboxMessages.FirstOrDefault(x => x.PersonId == person2.PersonId);
        var addWelshROutboxMessage3 = welshROutboxMessages.FirstOrDefault(x => x.PersonId == person3.PersonId);
        var addWelshROutboxMessage4 = welshROutboxMessages.FirstOrDefault(x => x.PersonId == person4.PersonId);
        Assert.Null(addWelshROutboxMessage1);
        Assert.Null(addWelshROutboxMessage2);
        Assert.NotNull(addWelshROutboxMessage3);
        Assert.NotNull(addWelshROutboxMessage4);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.NotEmpty(integrationTransaction.dfeta_FailureMessage);
        Assert.Contains(expectedValueMessage1, integrationTransaction.dfeta_FailureMessage, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(expectedValueMessage2, integrationTransaction.dfeta_FailureMessage, StringComparison.InvariantCultureIgnoreCase);
    }

    [Theory]
    [InlineData("67")]
    [InlineData("68")]
    [InlineData("69")]
    [InlineData("102")]
    public async Task EwcWalesImportJobQts_NonPermittedQtsStatusWhenQtsDatePast_ReturnsError(string qtsStatus)
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var person1 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person1.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn1},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},{qtsStatus},04/04/2024,,,,,,,,,,,,,,,,,,,,,,,,";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);
        var itrRecords = ctx.dfeta_integrationtransactionrecordSet.Where(i => i.GetAttributeValue<Guid>(dfeta_integrationtransactionrecord.Fields.dfeta_IntegrationTransactionId) == integrationTransaction.Id);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Contains("Qts Status can only be 71 or 49 when qts date is on or past 01/02/2023", item1.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Fail, item1.StatusCode);
            });
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Contains("Qts Status can only be 71 or 49 when qts date is on or past 01/02/2023", integrationTransaction.dfeta_FailureMessage);
    }

    [Theory]
    [InlineData("49")]
    [InlineData("71")]
    public async Task EwcWalesImportJobQts_PermittedQtsStatusWhenQtsDatePast_ReturnsSuccess(string qtsStatus)
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var person1 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person1.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn1},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},{qtsStatus},04/04/2024,,,,,,,,,,,,,,,,,,,,,,,,";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);

        var welshROutboxMessage = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddWelshRMessage))
            .Select(x => Assert.IsType<AddWelshRMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        //var setInductionOutboxMessage = ctx.dfeta_TrsOutboxMessageSet
        //    .Where(x => x.dfeta_MessageName == nameof(SetInductionRequiredToCompleteMessage) || x.dfeta_MessageName == nameof(AddWelshRMessage))
        //    .Select(x => Assert.IsType<SetInductionRequiredToCompleteMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
        //    .ToArray();
        //var minductionessage = setInductionOutboxMessage.Single(x => x.PersonId == person1.PersonId);
        var welshrmessage = welshROutboxMessage.Single(x => x.PersonId == person1.PersonId);
        Assert.Equal(person1.PersonId, welshrmessage.PersonId);
        //Assert.Equal(person1.PersonId, minductionessage.PersonId);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
    }

    [Theory]
    [InlineData("49")]
    [InlineData("67")]
    [InlineData("68")]
    [InlineData("69")]
    [InlineData("71")]
    public async Task EwcWalesImportJobQts_PermittedQtsStatusWhenQtsDateBefore_ReturnsSuccess(string qtsStatus)
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var person1 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn1 = person1.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn1},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},{qtsStatus},04/04/1999,,,,,,,,,,,,,,,,,,,,,,,,";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.Single(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == integrationTransactionId);

        var welshROutboxMessage = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddWelshRMessage))
            .Select(x => Assert.IsType<AddWelshRMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        //var setInductionOutboxMessage = ctx.dfeta_TrsOutboxMessageSet
        //    .Where(x => x.dfeta_MessageName == nameof(SetInductionRequiredToCompleteMessage) || x.dfeta_MessageName == nameof(AddWelshRMessage))
        //    .Select(x => Assert.IsType<SetInductionRequiredToCompleteMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
        //    .ToArray();
        //var minductionessage = setInductionOutboxMessage.Single(x => x.PersonId == person1.PersonId);
        var welshrmessage = welshROutboxMessage.Single(x => x.PersonId == person1.PersonId);
        Assert.Equal(person1.PersonId, welshrmessage.PersonId);
        //Assert.Equal(person1.PersonId, minductionessage.PersonId);
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
        var outboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddInductionExemptionMessage))
            .Select(x => Assert.IsType<AddInductionExemptionMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var message = outboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.NotNull(message);
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Empty(item1.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Success, item1.StatusCode);
            });
        Assert.Equal(InductionExemptionReason.PassedInWalesId, message.ExemptionReasonId);
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
        Assert.NotNull(integrationTransaction);
        Assert.Collection(itrRecords,
            item1 =>
            {
                Assert.Contains($"For TRN {person.Trn!} Date of Birth does not match with the existing record.", item1.dfeta_FailureMessage);
                Assert.Equal(dfeta_integrationtransactionrecord_StatusCode.Fail, item1.StatusCode);
            });
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.NotEmpty(integrationTransaction.dfeta_FailureMessage);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_ValidRowWithoutInductionStatus_ReturnsSuccessAndExpectedCounts()
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
        var outboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddInductionExemptionMessage))
            .Select(x => Assert.IsType<AddInductionExemptionMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var message = outboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.Equal(InductionExemptionReason.PassedInWalesId, message.ExemptionReasonId);
        Assert.NotNull(integrationTransaction);
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
        var outboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddInductionExemptionMessage))
            .Select(x => Assert.IsType<AddInductionExemptionMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var message = outboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.Equal(InductionExemptionReason.PassedInWalesId, message.ExemptionReasonId);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task EwcWalesImportJobInduction_WithInductionstatusThatCannotBeUpdated_ReturnsError(InductionStatus status)
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
            x.WithInductionStatus(builder => builder.WithStatus(status));
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
        var outboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddInductionExemptionMessage))
            .Select(x => Assert.IsType<AddInductionExemptionMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var message = outboxMessages.Where(x => x.PersonId == person.PersonId).SingleOrDefault();
        Assert.Null(message);
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
        var task = ctx.TaskSet.Single(i => i.GetAttributeValue<Guid>(Dqt.Models.Task.Fields.RegardingObjectId) == person.PersonId);
        var outboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddInductionExemptionMessage))
            .Select(x => Assert.IsType<AddInductionExemptionMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var message = outboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.Equal(InductionExemptionReason.PassedInWalesId, message.ExemptionReasonId);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
        Assert.NotNull(task);
        Assert.Equal(expectedTaskDescription, task.Description);
        Assert.Equal(expectedTaskSubject, task.Subject);
        Assert.Equal(expectedTaskCategory, task.Category);
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_WithRequiredToCompleteInductionStatus_UpdatesInductionStatus()
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
            x.WithInductionStatus(builder => builder.WithStatus(InductionStatus.RequiredToComplete));
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
        var outboxMessages = ctx.dfeta_TrsOutboxMessageSet
            .Where(x => x.dfeta_MessageName == nameof(AddInductionExemptionMessage))
            .Select(x => Assert.IsType<AddInductionExemptionMessage>(MessageSerializer.DeserializeMessage(x.dfeta_Payload, x.dfeta_MessageName)))
            .ToArray();
        var message = outboxMessages.Single(x => x.PersonId == person.PersonId);
        Assert.Equal(InductionExemptionReason.PassedInWalesId, message.ExemptionReasonId);
        Assert.Equal(expectedTotalRowCount, integrationTransaction.dfeta_TotalCount);
        Assert.Equal(expectedSuccessCount, integrationTransaction.dfeta_SuccessCount);
        Assert.Equal(expectedDuplicateRowCount, integrationTransaction.dfeta_DuplicateCount);
        Assert.Equal(expectedFailureRowCount, integrationTransaction.dfeta_FailureCount);
        Assert.Empty(integrationTransaction.dfeta_FailureMessage);
    }
}

public class EwcWalesImportJobFixture : IAsyncLifetime
{
    public EwcWalesImportJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        IServiceProvider provider,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
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
            loggerFactory.CreateLogger<TrsDataSyncHelper>(),
            BlobStorageFileService.Object,
            configuration);

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
        MessageSerializer = ActivatorUtilities.CreateInstance<MessageSerializer>(provider);
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

    public MessageSerializer MessageSerializer { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();
}
