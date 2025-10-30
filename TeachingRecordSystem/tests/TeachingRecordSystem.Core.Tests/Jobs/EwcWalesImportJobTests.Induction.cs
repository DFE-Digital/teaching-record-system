using System.Globalization;
using System.Text;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public partial class EwcWalesImportJobTests
{
    [Fact]
    public async Task EwcWalesImportJobInduction_WithActiveAlert_ImportsInductionFileSuccessfully()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var holdDate = new DateOnly(2016, 12, 01);
        var inductionStartDate = new DateOnly(2018, 09, 17);
        var passDate = new DateOnly(2019, 09, 28);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.WelshRId, holdDate);
            x.WithAlert();
        });
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{inductionStartDate.ToString("dd/MM/yyyy")},{passDate.ToString("dd/MM/yyyy")},,,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var contact = await dbContext.Persons.Include(x => x.Qualifications).SingleAsync(x => x.PersonId == person.PersonId);
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).FirstAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Equal(true, item1.HasActiveAlert);
                    Assert.NotNull(item1.RowData);
                });
            Assert.Equal(InductionStatus.Exempt, contact.InductionStatus);
            Assert.Contains(InductionExemptionReason.PassedInWalesId, contact.InductionExemptionReasonIds);
        });
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_WithQtls_ImportsInductionFileSuccessfully()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var qtlsDate = Clock.Today.AddYears(-2);
        var holdsDate = Clock.Today.AddYears(-2).AddDays(1);
        var startDate = Clock.Today.AddYears(-1).AddDays(1);
        var passDate = Clock.Today.AddYears(-1).AddDays(2);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.QtlsAndSetMembershipId)
                .WithHoldsFrom(holdsDate)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithInductionExemption(true));
            x.WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.WelshRId, holdsDate);
        });
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{startDate.ToString("dd/MM/yyyy")},{passDate.ToString("dd/MM/yyyy")},,,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var contact = await dbContext.Persons.Include(x => x.Qualifications).SingleAsync(x => x.PersonId == person.PersonId);
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).FirstAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Equal(false, item1.HasActiveAlert);
                    Assert.NotNull(item1.RowData);
                });
            Assert.Equal(InductionStatus.Exempt, contact.InductionStatus);
            var exemptionReasons = contact.GetAllInductionExemptionReasonIds();
            Assert.Contains(InductionExemptionReason.PassedInWalesId, exemptionReasons);
            Assert.Contains(InductionExemptionReason.QtlsId, exemptionReasons);
        });
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_ImportsInductionFileSuccessfully()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 1;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 0;
        var holdsFrom = new DateOnly(2000, 01, 01);
        var startDate = new DateOnly(2014, 09, 17);
        var passDate = new DateOnly(2017, 09, 28);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.WelshRId, holdsFrom);
        });
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{startDate.ToString("dd/MM/yyyy")},{passDate.ToString("dd/MM/yyyy")},,,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var contact = await dbContext.Persons.Include(x => x.Qualifications).SingleAsync(x => x.PersonId == person.PersonId);
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).FirstAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(person.PersonId, item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Success, item1.Status);
                    Assert.Equal(false, item1.HasActiveAlert);
                    Assert.NotNull(item1.RowData);
                });
            Assert.Equal(InductionStatus.Exempt, contact.InductionStatus);
            Assert.Contains(InductionExemptionReason.PassedInWalesId, contact.InductionExemptionReasonIds);
        });
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_DateOfBirthDoesNotMatch_FailsAndReturnsExpectedCounts()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var person = await TestData.CreatePersonAsync();
        var trn1 = person.Trn;
        var inductionStartDate = new DateOnly(2024, 05, 01);
        var inductionPassDate = new DateOnly(2024, 10, 07);
        var dob = "01/09/1977";
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{person.Trn},Keri Louise Lyddon,Nicholas,{dob},{inductionStartDate.ToString()},{inductionPassDate.ToString()},,Pembrokeshire Local Authority,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var contact = await dbContext.Persons.Include(x => x.Qualifications).SingleAsync(x => x.PersonId == person.PersonId);
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.Contains($"For TRN {person.Trn} Date of Birth does not match with the existing record.", item1.FailureMessage!);
                    Assert.Null(item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Failure, item1.Status);
                    Assert.NotNull(item1.RowData);
                });
            Assert.Equal(InductionStatus.None, contact.InductionStatus);
        });
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_InductionPassedDateBeforeQtsDate_ReturnsError()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var inductionStartDate = new DateOnly(1999, 09, 17);
        var inductionPassDate = new DateOnly(1999, 09, 28);
        var holdsFrom = new DateOnly(2000, 11, 04);
        var qtsDate = new DateOnly(2000, 10, 05);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithQts(qtsDate);
            x.WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.WelshRId, holdsFrom);
        });
        var expectedValueMessage = $"Induction passed date cannot be before Qts Date.";
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{inductionStartDate.ToString("dd/MM/yyyy")},{inductionPassDate.ToString("dd/MM/yyyy")},,,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var contact = await dbContext.Persons.Include(x => x.Qualifications).SingleAsync(x => x.PersonId == person.PersonId);
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).FirstAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.Contains(expectedValueMessage, item1.FailureMessage);
                    Assert.NotNull(item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Failure, item1.Status);
                    Assert.NotNull(item1.RowData);
                });

            // unchanged from induction import
            Assert.Equal(InductionStatus.RequiredToComplete, contact.InductionStatus);
        });
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_WithInvalidTRN_ReturnsExpectedCounts()
    {
        // Arrange
        var expectedTotalRowCount = 1;
        var expectedSuccessCount = 0;
        var expectedDuplicateRowCount = 0;
        var expectedFailureRowCount = 1;
        var inductionStartDate = new DateTime(2024, 05, 01);
        var inductionPassDate = new DateTime(2024, 10, 07);
        var invalidTRN = "invalid";
        var expectedFailureMessage = $"Teacher with TRN {invalidTRN} was not found.";
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{invalidTRN},Keri Louise Lyddon,Nicholas,01/01/2024,{inductionStartDate.ToString()},{inductionPassDate.ToString()},,,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).FirstOrDefaultAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.NotNull(it);
            Assert.Collection(it.IntegrationTransactionRecords!,
                item1 =>
                {
                    Assert.Contains(expectedFailureMessage, item1.FailureMessage);
                    Assert.Null(item1.PersonId);
                    Assert.Equal(IntegrationTransactionRecordStatus.Failure, item1.Status);
                    Assert.NotNull(item1.RowData);
                });

            Assert.Equal(expectedTotalRowCount, it.TotalCount);
            Assert.Equal(expectedSuccessCount, it.SuccessCount);
            Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
            Assert.Equal(expectedFailureRowCount, it.FailureCount);
        });
    }

    [Fact]
    public async Task EwcWalesImportJobInduction_WithRequiredToCompleteInductionStatus_UpdatesInductionStatus()
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
            x.WithHoldsRouteToProfessionalStatus(RouteToProfessionalStatusType.WelshRId, passDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        });

        var trn = person.Trn;
        var updatedStartDate = DateTime.ParseExact("17/09/2019", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var updatedPassDate = DateTime.ParseExact("28/09/2020", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth.ToString("dd/MM/yyyy")},{updatedStartDate.ToString("dd/MM/yyyy")},{updatedPassDate.ToString("dd/MM/yyyy")},,,,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await Job.ImportAsync("IND", reader);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
            {
                var contact = await dbContext.Persons.Include(x => x.Qualifications).SingleAsync(x => x.PersonId == person.PersonId);
                var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).FirstAsync(x => x.IntegrationTransactionId == integrationTransactionId);
                Assert.Equal(expectedTotalRowCount, it.TotalCount);
                Assert.Equal(expectedSuccessCount, it.SuccessCount);
                Assert.Equal(expectedDuplicateRowCount, it.DuplicateCount);
                Assert.Equal(expectedFailureRowCount, it.FailureCount);
                Assert.Equal(InductionStatus.Exempt, contact.InductionStatus);
            });
    }
}
