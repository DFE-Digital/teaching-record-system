using System.Text;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public partial class EwcWalesImportJobTests
{
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
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString(QtsImporter.DATE_FORMAT)},{qtsStatus},04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Collection(it.IntegrationTransactionRecords!, item1 =>
            {
                Assert.Contains(expectedErrorMessage, item1.FailureMessage);
                Assert.Equal(person.PersonId, item1.PersonId);
                Assert.NotNull(item1.RowData);
            });
            Assert.NotNull(it);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Equal(fileName, it.FileName);
        });
    }

    [Theory]
    [InlineData("49")]
    [InlineData("71")]
    public async Task EwcWalesImportJobQts_WelshRAfterECDirectiveChange_Format1ImportsQtsFileSuccessfully(string qtsStatus)
    {
        // Arrange
        var awardedDate = Clock.Today.AddDays(-10);
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString(QtsImporter.DATE_FORMAT)},{qtsStatus},{awardedDate.ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            var contact = dbContext.Persons.Include(x => x.Qualifications).Single(x => x.PersonId == person.PersonId);

            Assert.Equal(InductionStatus.RequiredToComplete, contact.InductionStatus);
            Assert.Equal(awardedDate, contact.QtsDate);
            var qualification = (RouteToProfessionalStatus)contact.Qualifications!.First(x => x.QualificationType == QualificationType.RouteToProfessionalStatus);
            Assert.NotNull(qualification);
            Assert.Equal(RouteToProfessionalStatusType.WelshRId, qualification.RouteToProfessionalStatusType!.RouteToProfessionalStatusTypeId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Equal(fileName, it.FileName);
            Assert.Collection(it.IntegrationTransactionRecords!, item1 =>
            {
                Assert.Empty(item1.FailureMessage!);
                Assert.Equal(person.PersonId, item1.PersonId);
                Assert.NotNull(item1.RowData);
            });
        });
    }

    [Theory]
    [InlineData("49")]
    [InlineData("71")]
    public async Task EwcWalesImportJobQts_WelshRBeforeECDirectiveChange_Format1ImportsQtsFileSuccessfully(string qtsStatus)
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
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString(QtsImporter.DATE_FORMAT)},{qtsStatus},{awardedDate.ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            var contact = dbContext.Persons.Include(x => x.Qualifications).Single(x => x.PersonId == person.PersonId);

            Assert.Equal(InductionStatus.RequiredToComplete, contact.InductionStatus);
            Assert.Equal(awardedDate, contact.QtsDate);
            var qualification = (RouteToProfessionalStatus)contact.Qualifications!.First(x => x.QualificationType == QualificationType.RouteToProfessionalStatus);
            Assert.NotNull(qualification);
            Assert.Equal(RouteToProfessionalStatusType.WelshRId, qualification.RouteToProfessionalStatusType!.RouteToProfessionalStatusTypeId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Equal(fileName, it.FileName);
            Assert.Collection(it.IntegrationTransactionRecords!, item1 =>
            {
                Assert.Empty(item1.FailureMessage!);
                Assert.Equal(person.PersonId, item1.PersonId);
                Assert.NotNull(item1.RowData);
            });
        });
    }

    [Theory]
    [InlineData("49")]
    [InlineData("71")]
    public async Task EwcWalesImportJobQts_BeforeECDirectiveChange_Format1ImportsQtsFileSuccessfully(string qtsStatus)
    {
        // Arrangev
        var awardedDate = new DateOnly(2011, 01, 04);
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString(QtsImporter.DATE_FORMAT)},{qtsStatus},{awardedDate.ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            var contact = dbContext.Persons.Include(x => x.Qualifications).Single(x => x.PersonId == person.PersonId);

            Assert.Equal(InductionStatus.RequiredToComplete, contact.InductionStatus);
            Assert.Equal(awardedDate, contact.QtsDate);
            var qualification = (RouteToProfessionalStatus)contact.Qualifications!.First(x => x.QualificationType == QualificationType.RouteToProfessionalStatus);
            Assert.NotNull(qualification);
            Assert.Equal(RouteToProfessionalStatusType.WelshRId, qualification.RouteToProfessionalStatusType!.RouteToProfessionalStatusTypeId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Equal(fileName, it.FileName);
            Assert.Collection(it.IntegrationTransactionRecords!, item1 =>
            {
                Assert.Empty(item1.FailureMessage!);
                Assert.Equal(person.PersonId, item1.PersonId);
                Assert.NotNull(item1.RowData);
            });
        });
    }

    [Fact]
    public async Task EwcWalesImportJobQts_ECDirectiveBeforeECDirectiveChange_Format1ImportsQtsFileSuccessfully()
    {
        // Arrangev
        var qtsStatus = "67";
        var awardedDate = new DateOnly(2014, 01, 04);
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString(QtsImporter.DATE_FORMAT)},{qtsStatus},{awardedDate.ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            var contact = dbContext.Persons.Include(x => x.Qualifications).Single(x => x.PersonId == person.PersonId);

            Assert.Equal(InductionStatus.RequiredToComplete, contact.InductionStatus);
            Assert.Equal(awardedDate, contact.QtsDate);
            var qualification = (RouteToProfessionalStatus)contact.Qualifications!.First(x => x.QualificationType == QualificationType.RouteToProfessionalStatus);
            Assert.NotNull(qualification);
            Assert.Equal(RouteToProfessionalStatusType.ECDirective, qualification.RouteToProfessionalStatusType!.RouteToProfessionalStatusTypeId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Equal(fileName, it.FileName);
            Assert.Collection(it.IntegrationTransactionRecords!, item1 =>
            {
                Assert.Empty(item1.FailureMessage!);
                Assert.Equal(person.PersonId, item1.PersonId);
                Assert.NotNull(item1.RowData);
            });
        });
    }

    [Fact]
    public async Task EwcWalesImportJobQts_Format2ImportsQtsFileSuccessfully()
    {
        // Arrange
        string qtsStatus = "71";
        var awardedDate = new DateOnly(2014, 01, 02);
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var fileName = "QTS.csv";
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,CODE,QTS_DATE\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString(QtsImporter.DATE_FORMAT)},{qtsStatus},{awardedDate.ToString(QtsImporter.DATE_FORMAT)}\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            var contact = dbContext.Persons.Include(x => x.Qualifications).Single(x => x.PersonId == person.PersonId);
            Assert.Equal(InductionStatus.RequiredToComplete, contact.InductionStatus);
            Assert.Equal(awardedDate, contact.QtsDate);
            var qualification = contact.Qualifications!.First(x => x.QualificationType == QualificationType.RouteToProfessionalStatus);
            Assert.NotNull(qualification);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Equal(fileName, it.FileName);
            Assert.Collection(it.IntegrationTransactionRecords!, item1 =>
            {
                Assert.NotNull(item1.PersonId);
                Assert.Empty(item1.FailureMessage!);
                Assert.Equal(person.PersonId, item1.PersonId);
                Assert.NotNull(item1.RowData);
            });
            Assert.Equal(InductionStatus.RequiredToComplete, contact.InductionStatus);
        });
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

        // Act
        var integrationTransactionId = await Job.ImportAsync(fileName, reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            var contact = dbContext.Persons.Include(x => x.Qualifications).Single(x => x.PersonId == person.PersonId);
            var qualification = contact.Qualifications!.First(x => x.QualificationType == QualificationType.RouteToProfessionalStatus);
            Assert.NotNull(qualification);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Equal(fileName, it.FileName);
            Assert.Collection(it.IntegrationTransactionRecords!, item1 =>
            {
                Assert.NotNull(item1.PersonId);
                Assert.Equal(person.PersonId, item1.PersonId);
                Assert.Empty(item1.FailureMessage!);
                Assert.True(item1.HasActiveAlert);
                Assert.NotNull(item1.RowData);
            });
            Assert.Equal(InductionStatus.RequiredToComplete, contact.InductionStatus);
        });
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
        var person2AwardedDate = new DateOnly(2014, 04, 04);
        var person1 = await TestData.CreatePersonAsync(p => p.WithTrn()
            .WithQts()
            .WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.WelshRId, person1AwardedDate));
        var person2 = await TestData.CreatePersonAsync(p => p.WithTrn());
        var expectedValueMessage = $"For TRN {person1.Trn} Date of Birth does not match";
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{person1.Trn},firstname,lastname,{person1.DateOfBirth.AddYears(-1).ToString(QtsImporter.DATE_FORMAT)},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{person2.Trn},firstname,lastname,{person2.DateOfBirth.ToString(QtsImporter.DATE_FORMAT)},49,{person2AwardedDate.ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            var contact1 = dbContext.Persons.Include(x => x.Qualifications).Single(x => x.PersonId == person1.PersonId);
            var contact2 = dbContext.Persons.Include(x => x.Qualifications).Single(x => x.PersonId == person2.PersonId);
            var qualification1 = contact1.Qualifications!.First(x => x.QualificationType == QualificationType.RouteToProfessionalStatus);
            var qualification2 = contact2.Qualifications!.First(x => x.QualificationType == QualificationType.RouteToProfessionalStatus);
            Assert.NotNull(qualification1);
            Assert.NotNull(qualification2);
            Assert.Equal(person1AwardedDate, contact1.QtsDate);
            Assert.Equal(person2AwardedDate, contact2.QtsDate);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item2 =>
                {
                    Assert.Contains(expectedValueMessage, item2.FailureMessage);
                    Assert.Null(item2.PersonId);
                    Assert.NotNull(item2.RowData);
                },
                item1 =>
                {
                    Assert.Empty(item1.FailureMessage!);
                    Assert.Equal(person2.PersonId, item1.PersonId);
                    Assert.NotNull(item1.RowData);
                });
            Assert.Equal(InductionStatus.RequiredToComplete, contact2.InductionStatus);
            Assert.Equal(InductionStatus.RequiredToComplete, contact1.InductionStatus);
        });
    }

    [Fact]
    public async Task EwcWalesImportJobQts_MultipleSuccessMultipleFailure_ReturnsExpectedCounts()
    {
        // Arrange
        var person1AwardedDate = new DateOnly(2011, 01, 04);
        var person2AwardedDate = new DateOnly(2000, 01, 07);
        var person3AwardedDate = new DateOnly(2001, 06, 07);
        var person4AwardedDate = new DateOnly(2000, 03, 08);
        var person1QtsDate = new DateOnly(2000, 05, 04);
        var person2QtsDate = new DateOnly(2001, 08, 11);
        var person1 = await TestData.CreatePersonAsync(p => p.WithTrn()
            .WithQts(person1QtsDate));
        var person2 = await TestData.CreatePersonAsync(p => p.WithTrn()
            .WithQts(person2QtsDate));
        var person3 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var person4 = await TestData.CreatePersonAsync(x => x.WithTrn());
        var expectedTotalRowCount = 4;
        var expectedSuccessCount = 2;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 2;
        var expectedValueMessage1 = $"For TRN {person1.Trn} Date of Birth does not match";
        var expectedValueMessage2 = $"For TRN {person2.Trn} Date of Birth does not match";
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{person1.Trn},firstname,lastname,{person1.DateOfBirth.AddDays(1).ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{person2.Trn},firstname,lastname,{person2.DateOfBirth.AddDays(-10).ToString("dd/MM/yyyy")},49,04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n{person3.Trn},firstname,lastname,{person3.DateOfBirth.ToString("dd/MM/yyyy")},49,{person3AwardedDate.ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,\r\n{person4.Trn},firstname,lastname,{person4.DateOfBirth.ToString("dd/MM/yyyy")},49,{person4AwardedDate.ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            var contact1 = await dbContext.Persons.SingleAsync(x => x.PersonId == person1.PersonId);
            var contact2 = await dbContext.Persons.SingleAsync(x => x.PersonId == person2.PersonId);
            var contact3 = await dbContext.Persons.SingleAsync(x => x.PersonId == person3.PersonId);
            var contact4 = await dbContext.Persons.SingleAsync(x => x.PersonId == person4.PersonId);
            Assert.Collection(it.IntegrationTransactionRecords!,
                  item1 =>
                  {
                      Assert.Contains(expectedValueMessage1, item1.FailureMessage);
                      Assert.Equal(IntegrationTransactionRecordStatus.Failure, item1.Status);
                      Assert.NotNull(item1.RowData);
                  },
                 item2 =>
                 {
                     Assert.Contains(expectedValueMessage2, item2.FailureMessage);
                     Assert.Equal(IntegrationTransactionRecordStatus.Failure, item2.Status);
                     Assert.NotNull(item2.RowData);
                 },
                 item3 =>
                 {
                     Assert.Empty(item3.FailureMessage!);
                     Assert.Equal(IntegrationTransactionRecordStatus.Success, item3.Status);
                     Assert.NotNull(item3.RowData);
                 },
                 item4 =>
                 {
                     Assert.Empty(item4.FailureMessage!);
                     Assert.Equal(IntegrationTransactionRecordStatus.Success, item4.Status);
                     Assert.NotNull(item4.RowData);
                 });

            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Equal(person1QtsDate, contact1.QtsDate);
            Assert.Equal(person2QtsDate, contact2.QtsDate);
            Assert.Equal(person3AwardedDate, contact3.QtsDate);
            Assert.Equal(person4AwardedDate, contact4.QtsDate);
        });
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
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.Contains("Qts Status can only be 71 or 49 when qts date is on or past 01/02/2023", item1.FailureMessage);
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person1.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Failure, item1.Status);
                    Assert.NotNull(item1.RowData);
                });
        });
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
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn1},firstname,lastname,{person1.DateOfBirth.ToString("dd/MM/yyyy")},{qtsStatus},{Clock.Today.AddDays(-10).ToString(QtsImporter.DATE_FORMAT)},,,,,,,,,,,,,,,,,,,,,,,,";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("QTS", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.Empty(item1.FailureMessage!);
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person1.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.NotNull(item1.RowData);
                });
        });
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
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.Empty(item1.FailureMessage!);
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person1.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.NotNull(item1.RowData);
                });
        });
    }
}
