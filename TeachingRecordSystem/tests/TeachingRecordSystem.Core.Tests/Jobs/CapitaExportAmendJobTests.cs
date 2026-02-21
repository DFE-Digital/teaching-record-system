using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class CapitaExportAmendJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    private const int EXPECTED_ROW_LENGTH = 86;

    [Fact]
    public async Task Execute_WithNoChanges_ReturnsExpectedCounts()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var expectedSuccessCount = 0;
            var expectedFailureCount = 0;
            var expectedTotalCount = 0;
            var expectedDuplicateCount = 0;

            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));

            // Act
            var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);
            var integrationTransaction = dbContext.IntegrationTransactions
                .Include(x => x.IntegrationTransactionRecords)
                .Single(x => x.IntegrationTransactionId == integrationTransactionJobId);

            // Assert
            Assert.NotNull(integrationTransaction);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
            Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
            Assert.Equal(expectedTotalCount, integrationTransaction.TotalCount);
            Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
            Assert.Empty(integrationTransaction.IntegrationTransactionRecords!);
            Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
        });

    }


    [Fact]
    public async Task Execute_WithDobChanges_ReturnsExpectedCounts()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportAmendJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-3).AddHours(-2).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            await dbContext.SaveChangesAsync();
            var expectedSuccessCount = 1;
            var expectedFailureCount = 0;
            var expectedTotalCount = 1;
            var expectedDuplicateCount = 0;
            var lastName = "xxxxxx";
            var updatedDateOfBirth = new DateOnly(1977, 04, 02);
            var person = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(lastName);
                x.WithDateOfBirth(new DateOnly(1983, 01, 07));
                x.WithGender(Gender.Male);
                x.WithCreatedByTps(true);
            });
            var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);

            var personService = new PersonService(
                dbContext,
                Clock,
                new TestEventPublisher());
            var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow.AddHours(-1), SystemUser.SystemUserId);
            var updateResult = await personService.UpdatePersonDetailsAsync(new(
                person.PersonId,
                new()
                {
                    DateOfBirth = Option.Some<DateOnly?>(updatedDateOfBirth)
                },
                null,
                null),
                processContext);
            var personUpdatedEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow.AddHours(-1),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                PersonAttributes = updateResult.PersonDetails.ToEventModel(),
                OldPersonAttributes = updateResult.OldPersonDetails.ToEventModel(),
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = updateResult.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
            };
            dbContext.AddEventWithoutBroadcast(personUpdatedEvent!);
            await dbContext.SaveChangesAsync();

            // Act
            var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);
            var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionJobId);

            // Assert
            Assert.NotNull(integrationTransaction);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
            Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
            Assert.Equal(expectedTotalCount, integrationTransaction.TotalCount);
            Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);

            var expectedRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastName}" +
                              $"{new string(' ', 1)}" +
                              $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 9)}" +
                              $"{new string(' ', 44)}" +
                              $"1211ZE1*";
            Assert.NotNull(integrationTransaction);
            Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedRow, trsPerson));
        });

    }


    [Fact]
    public async Task Execute_WithNationalInsuranceNumberChanges_ReturnsExpectedCounts()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportAmendJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-3).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            await dbContext.SaveChangesAsync();
            var expectedSuccessCount = 1;
            var expectedFailureCount = 0;
            var expectedTotalCount = 1;
            var expectedDuplicateCount = 0;
            var lastName = "xxxxxx";
            var updatedNationalInsuranceNumber = NationalInsuranceNumber.Parse(Faker.Identification.UkNationalInsuranceNumber());
            var person = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(lastName);
                x.WithDateOfBirth(new DateOnly(1983, 01, 07));
                x.WithGender(Gender.Male);
                x.WithCreatedByTps(true);
            });
            var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
            var personService = new PersonService(
                dbContext,
                Clock,
                new TestEventPublisher());
            var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow.AddHours(-1), SystemUser.SystemUserId);
            var updateResult = await personService.UpdatePersonDetailsAsync(new(
                person.PersonId,
                new()
                {
                    NationalInsuranceNumber = Option.Some<NationalInsuranceNumber?>(updatedNationalInsuranceNumber),
                },
                null,
                null),
                processContext);
            var personUpdatedEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow.AddHours(-1),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                PersonAttributes = updateResult.PersonDetails.ToEventModel(),
                OldPersonAttributes = updateResult.OldPersonDetails.ToEventModel(),
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = updateResult.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
            };
            dbContext.AddEventWithoutBroadcast(personUpdatedEvent!);
            await dbContext.SaveChangesAsync();

            // Act
            var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);
            var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionJobId);

            // Assert
            Assert.NotNull(integrationTransaction);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
            Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
            Assert.Equal(expectedTotalCount, integrationTransaction.TotalCount);
            Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
            var expectedRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastName}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 7)}" +
                              $"{new string(' ', 1)}" +
                              $"{trsPerson.NationalInsuranceNumber}" +
                              $"{new string(' ', 44)}" +
                              $" 321ZE2*";
            Assert.NotNull(integrationTransaction);
            Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedRow, trsPerson));
        });

    }


    [Fact]
    public async Task Execute_WithNiandDobUpdates_ReturnsExpectedCounts()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportAmendJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-3).AddHours(-5).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            await dbContext.SaveChangesAsync();
            var expectedSuccessCount = 2;
            var expectedFailureCount = 0;
            var expectedTotalCount = 2;
            var expectedDuplicateCount = 0;
            var lastName = "xxxxxx";
            var updatedNationalInsuranceNumber = NationalInsuranceNumber.Parse(Faker.Identification.UkNationalInsuranceNumber());
            var updatedDateOfBirth = new DateOnly(1977, 02, 15);
            var person = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(lastName);
                x.WithDateOfBirth(new DateOnly(1983, 01, 07));
                x.WithGender(Gender.Male);
                x.WithCreatedByTps(true);
            });
            var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
            var personService = new PersonService(
                dbContext,
                Clock,
                new TestEventPublisher());
            var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, Clock.UtcNow.AddHours(-1), SystemUser.SystemUserId);
            var updateResult = await personService.UpdatePersonDetailsAsync(new(
                person.PersonId,
                new()
                {
                    DateOfBirth = Option.Some<DateOnly?>(updatedDateOfBirth),
                    NationalInsuranceNumber = Option.Some<NationalInsuranceNumber?>(updatedNationalInsuranceNumber),
                },
                null,
                null),
                processContext);
            var personUpdatedEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                PersonAttributes = updateResult.PersonDetails.ToEventModel(),
                OldPersonAttributes = updateResult.OldPersonDetails.ToEventModel(),
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = updateResult.Changes.ToLegacyPersonDetailsUpdatedEventChanges()
            };
            dbContext.AddEventWithoutBroadcast(personUpdatedEvent!);
            await dbContext.SaveChangesAsync();

            // Act
            var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);
            var integrationTransaction = dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).Single(x => x.IntegrationTransactionId == integrationTransactionJobId);

            // Assert
            Assert.NotNull(integrationTransaction);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Equal(expectedSuccessCount, integrationTransaction.SuccessCount);
            Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
            Assert.Equal(expectedTotalCount, integrationTransaction.TotalCount);
            Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
            var expectedNIRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastName}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 7)}" +
                              $"{new string(' ', 1)}" +
                              $"{trsPerson.NationalInsuranceNumber}" +
                              $"{new string(' ', 44)}" +
                              $" 321ZE2*";

            var expectedDobRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastName}" +
                              $"{new string(' ', 1)}" +
                              $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 9)}" +
                              $"{new string(' ', 44)}" +
                              $"1211ZE1*";
            Assert.NotNull(integrationTransaction);
            Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNIRow, trsPerson));
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedDobRow, trsPerson));
        });

    }


    [Fact]
    public async Task GetPersonAmendedRow_WithLastNameExceedsSixCharacters_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));
            var lastName = new string('x', 25);
            var person = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(lastName);
                x.WithDateOfBirth(new DateOnly(1983, 01, 07));
                x.WithGender(Gender.Male);
            });
            var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
            var amendedPerson = new CapitaExportAmendJobResult()
            {
                Trn = trsPerson.Trn,
                LastName = trsPerson.LastName,
                DateOfBirth = trsPerson.DateOfBirth,
                NationalInsuranceNumber = null,
                PersonId = trsPerson.PersonId,
                Gender = trsPerson.Gender,
                ChangeType = PersonAttributesChanges.DateOfBirth,
                Created = DateTime.UtcNow
            };

            // Act
            var row = job.GetPersonAmendedRow(amendedPerson, CapitaAmendExportType.DateOfBirth);

            // Assert
            var lastNameFixed = lastName.Substring(0, 6);
            var expectedRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastNameFixed}" +
                              $"{new string(' ', 1)}" +
                              $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 9)}" +
                              $"{new string(' ', 44)}" +
                              $"1211ZE1*";
            Assert.Equal(EXPECTED_ROW_LENGTH, row.Length);
            Assert.Equal(expectedRow, row);
        });

    }


    [Fact]
    public async Task GetPersonAmendedRow_WithLastNameLessThanSixCharacters_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));
            var lastName = "xx";
            var person = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(lastName);
                x.WithDateOfBirth(new DateOnly(1983, 01, 07));
                x.WithGender(Gender.Male);
            });
            var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
            var amendedPerson = new CapitaExportAmendJobResult()
            {
                Trn = trsPerson.Trn,
                LastName = trsPerson.LastName,
                DateOfBirth = trsPerson.DateOfBirth,
                NationalInsuranceNumber = null,
                PersonId = trsPerson.PersonId,
                Gender = trsPerson.Gender,
                ChangeType = PersonAttributesChanges.DateOfBirth,
                Created = DateTime.UtcNow
            };

            // Act
            var row = job.GetPersonAmendedRow(amendedPerson, CapitaAmendExportType.DateOfBirth);

            // Assert
            var lastNameFixed = lastName.PadRight(6);
            var expectedRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastNameFixed}" +
                              $"{new string(' ', 1)}" +
                              $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 9)}" +
                              $"{new string(' ', 44)}" +
                              $"1211ZE1*";
            Assert.Equal(EXPECTED_ROW_LENGTH, row.Length);
            Assert.Equal(expectedRow, row);
        });

    }


    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task GetPersonAmendedRow_WithDateOfBirthExportType_ReturnsExpectedContent(Gender gender)
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));
            var person = await TestData.CreatePersonAsync(x =>
            {
                x.WithDateOfBirth(new DateOnly(1983, 01, 07));
                x.WithGender(gender);
            });
            var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
            var amendedPerson = new CapitaExportAmendJobResult()
            {
                Trn = trsPerson.Trn,
                LastName = trsPerson.LastName,
                DateOfBirth = trsPerson.DateOfBirth,
                NationalInsuranceNumber = null,
                PersonId = trsPerson.PersonId,
                Gender = trsPerson.Gender,
                ChangeType = PersonAttributesChanges.DateOfBirth,
                Created = DateTime.UtcNow
            };

            // Act
            var row = job.GetPersonAmendedRow(amendedPerson, CapitaAmendExportType.DateOfBirth);

            // Assert
            var lastNameFixed = trsPerson.LastName.Length > 6
                ? trsPerson.LastName.Substring(0, 6)
                : trsPerson.LastName.PadRight(6);
            var expectedRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastNameFixed}" +
                              $"{new string(' ', 1)}" +
                              $"{trsPerson.DateOfBirth!.Value:ddMMyy*}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 9)}" +
                              $"{new string(' ', 44)}" +
                              $"1211ZE1*";
            Assert.Equal(EXPECTED_ROW_LENGTH, row.Length);
            Assert.Equal(expectedRow, row);
        });

    }


    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task GetPersonAmendedRow_WithNationalInsurance_ReturnsExpectedContent(Gender gender)
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));
            var person = await TestData.CreatePersonAsync(x =>
            {
                x.WithDateOfBirth(new DateOnly(1983, 01, 07));
                x.WithGender(gender);
                x.WithNationalInsuranceNumber(true);
            });
            var trsPerson = dbContext.Persons.Single(x => x.PersonId == person.PersonId);
            var amendedPerson = new CapitaExportAmendJobResult()
            {
                Trn = trsPerson.Trn,
                LastName = trsPerson.LastName,
                DateOfBirth = trsPerson.DateOfBirth,
                NationalInsuranceNumber = person.NationalInsuranceNumber,
                PersonId = trsPerson.PersonId,
                Gender = trsPerson.Gender,
                ChangeType = PersonAttributesChanges.NationalInsuranceNumber,
                Created = DateTime.UtcNow
            };

            // Act
            var row = job.GetPersonAmendedRow(amendedPerson, CapitaAmendExportType.NINumber);

            // Assert
            var lastNameFixed = trsPerson.LastName.Length > 6
                ? trsPerson.LastName.Substring(0, 6)
                : trsPerson.LastName.PadRight(6);
            var expectedRow = $"{trsPerson.Trn}" +
                              $"{(int)trsPerson.Gender!}" +
                              $"//" +
                              $"{lastNameFixed}" +
                              $"{new string(' ', 1)}" +
                              $"{new string(' ', 7)}" +
                              $"{new string(' ', 1)}" +
                              $"{person.NationalInsuranceNumber}" +
                              $"{new string(' ', 44)}" +
                              $" 321ZE2*";
            Assert.Equal(EXPECTED_ROW_LENGTH, row.Length);
            Assert.Equal(expectedRow, row);
        });
    }


    [Theory]
    [InlineData(CapitaAmendExportType.DateOfBirth, "1211ZE1*")]
    [InlineData(CapitaAmendExportType.NINumber, " 321ZE2*")]
    public async Task GetUpdateCode_ReturnsExpectedContent(CapitaAmendExportType exportType, string expectedUpdateCode)
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));

            // Act
            var updateCode = job.GetUpdateCode(exportType);

            // Assert
            Assert.Equal(expectedUpdateCode, updateCode);
        });

    }


    [Fact]
    public async Task GetFileName_ReturnsExpectedFileName()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportAmendJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportAmendJob>.Instance, dbContext, Clock, Options.Create(new CapitaTpsUserOption { CapitaTpsUserId = ApplicationUser.CapitaTpsImportUser.UserId }));
            var expectedFileName = $"Reg01_DTR_{Clock.UtcNow.ToString("yyyyMMdd")}_{Clock.UtcNow.ToString("HHmmss")}_Amend.txt"; ;

            // Act
            var fileName = job.GetFileName(Clock);

            // Assert
            Assert.Equal(expectedFileName, fileName);
        });

    }


    bool MatchesExpectedRowData(IntegrationTransactionRecord record, string expectedRowData, Person person) =>
        record.PersonId == person.PersonId &&
        record.FailureMessage == null &&
        record.Duplicate == null &&
        record.RowData != null &&
        record.RowData.Length == EXPECTED_ROW_LENGTH &&
        record.RowData == expectedRowData &&
        record.Status == IntegrationTransactionRecordStatus.Success;

    private static DataLakeServiceClient CreateDataLakeServiceClientMock()
    {
        var dataLakeServiceClientMock = new Mock<DataLakeServiceClient>();
        var fileSystemClientMock = new Mock<DataLakeFileSystemClient>();
        var dataLakeFileClientMock = new Mock<DataLakeFileClient>();

        dataLakeServiceClientMock
            .Setup(s => s.GetFileSystemClient(It.IsAny<string>()))
            .Returns(fileSystemClientMock.Object);
        fileSystemClientMock
            .Setup(f => f.CreateIfNotExistsAsync(
                It.IsAny<DataLakeFileSystemCreateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<Azure.Storage.Files.DataLake.Models.FileSystemInfo>>());
        fileSystemClientMock
            .Setup(f => f.GetFileClient(It.IsAny<string>()))
            .Returns(dataLakeFileClientMock.Object);
        dataLakeFileClientMock
            .Setup(f => f.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<PathInfo>>());

        return dataLakeServiceClientMock.Object;
    }
}
