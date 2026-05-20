using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging.Abstractions;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class CapitaExportNewJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    private const int ExpectedRowLength = 86;

    [Fact]
    public async Task GetNewPersons_CreatedAfterLastRunDate_ReturnsExpectedRecords()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportNewJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-3).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            await dbContext.SaveChangesAsync();
            var lastRunDate = Clock.UtcNow.AddDays(-2);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(Gender.Male);
            });
            var person2 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(Gender.Male);
            });

            // Act
            var newPersons = await job.GetNewPersonsAsync(lastRunDate);

            // Assert
            Assert.Contains(newPersons, p => p.Trn == person1.Trn);
            Assert.Contains(newPersons, p => p.Trn == person2.Trn);
        });
    }

    [Theory]
    [InlineData(Gender.NotAvailable)]
    [InlineData(Gender.Other)]
    public async Task GetNewPersons_WithNoneValidGenders_ReturnsExpectedRecords(Gender gender)
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportNewJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-3).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            await dbContext.SaveChangesAsync();
            var lastRunDate = DateTime.UtcNow.AddDays(-2);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(gender);
            });

            // Act
            var newPersons = await job.GetNewPersonsAsync(lastRunDate);

            // Assert
            Assert.DoesNotContain(newPersons, p => p.Trn == person1.Trn);
        });
    }

    [Fact]
    public async Task GetNewPersons_NoRecordsCreatedAfterLastRunDate_ReturnsExpectedRecords()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var lastRunDate = Clock.UtcNow.AddDays(1);

            // Act
            var newPersons = await job.GetNewPersonsAsync(lastRunDate);

            // Assert
            Assert.Empty(newPersons);
        });
    }

    [Fact]
    public async Task GetNewPersons_CreatedAferLastRunDateWithoutTrn_ReturnsExpectedRecords()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var lastRunDate = DateTime.UtcNow.AddDays(-2);
            var person1 = await TestData.CreatePersonAsync();
            var person2 = await TestData.CreatePersonAsync();

            // Act
            var newPersons = await job.GetNewPersonsAsync(lastRunDate);

            // Assert
            Assert.Empty(newPersons);
        });
    }

    [Theory]
    [InlineData(Gender.Male, "1")]
    [InlineData(Gender.Female, "2")]
    [InlineData(Gender.Other, " ")]
    [InlineData(Gender.NotAvailable, " ")]
    public async Task GetNewPersonAsStringRow_ValidPersonWithGender_ReturnsExpectedContent(Gender gender, string expectedGenderCode)
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(gender);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

            // Act
            var rowString = job.GetNewPersonAsStringRow(trsPerson, false);

            // Assert
            var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
            var expectedRowString = $"{trsPerson.Trn}" +
                $"{expectedGenderCode}" +
                $"{new string(' ', 9)}" +
                $"{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson.LastName.PadRight(17, ' ')}" +
                $"{new string(' ', 1)}" +
                $"{name}" +
                $"{new string(' ', 1)}" +
                $"{"1018Z981"}";
            Assert.NotNull(rowString);
            Assert.Equal(expectedRowString, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Theory]
    [InlineData(Gender.Male, "1")]
    [InlineData(Gender.Female, "2")]
    public async Task GetNewPersonAsStringRow_ValidPersonWithPreviousName_ReturnsExpectedContent(Gender gender, string expectedGenderCode)
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var newLastName = Faker.Name.Last();
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(gender);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            var personDetails = EventModels.PersonDetails.FromModel(trsPerson);
            var nameChangeEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow.AddYears(-2),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = trsPerson.PersonId,
                PersonAttributes = personDetails,
                OldPersonAttributes = personDetails,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.None
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = job.GetNewPersonAsStringRow(trsPerson, true);

            // Assert
            var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
            var expectedRowString = $"{trsPerson.Trn}" +
                $"{expectedGenderCode}" +
                $"{new string(' ', 9)}" +
                $"{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson.LastName.PadRight(17, ' ')}" +
                $"{new string('1', 1)}" + //1 if has a previous name
                $"{name}" +
                $"{new string(' ', 1)}" +
                $"{"1018Z981"}";
            Assert.NotNull(rowString);
            Assert.Equal(expectedRowString, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Theory]
    [InlineData("lastName")]
    [InlineData("superduperlonglengthlastname")]
    public async Task GetNewPersonAsStringRow_LastNameLengths_ReturnsExpectedContent(string lastName)
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            Gender gender = Gender.Male;
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(gender);
                x.WithLastName(lastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

            // Act
            var rowString = job.GetNewPersonAsStringRow(trsPerson, false);

            // Assert
            var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
            var expectedLastName = trsPerson.LastName.Length > 17 ? trsPerson.LastName.Substring(0, 17) : trsPerson.LastName.PadRight(17, ' ');
            var expectedRowString = $"{trsPerson.Trn}" +
                $"{(int)gender}" +
                $"{new string(' ', 9)}" +
                $"{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}" +
                $"{new string(' ', 1)}" +
                $"{expectedLastName}" +
                $"{new string(' ', 1)}" +
                $"{name}{new string(' ', 1)}" +
                $"{"1018Z981"}";
            Assert.NotNull(rowString);
            Assert.Equal(expectedRowString, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutDateOfBirth_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(Gender.Male);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            trsPerson.DateOfBirth = null;

            // Act
            var rowString = job.GetNewPersonAsStringRow(trsPerson, false);

            // Assert
            var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35, ' ');
            var expectedRowString = $"{trsPerson.Trn}" +
                $"{(int)Gender.Male}" +
                $"{new string(' ', 9)}" +
                $"{new string(' ', 6)}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson.LastName.PadRight(17, ' ')}" +
                $"{new string(' ', 1)}" +
                $"{name}" +
                $"{new string(' ', 1)}" +
                $"{"1018Z981"}";
            Assert.NotNull(rowString);
            Assert.Equal(expectedRowString, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutMiddleName_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(Gender.Male);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

            trsPerson.DateOfBirth = null;
            trsPerson.MiddleName = string.Empty;

            // Act
            var rowString = job.GetNewPersonAsStringRow(trsPerson, false);

            // Assert
            var name = $"{trsPerson.FirstName}".PadRight(35, ' ');
            var expectedRowString = $"{trsPerson.Trn}" +
                $"{(int)Gender.Male}" +
                $"{new string(' ', 9)}" +
                $"{new string(' ', 6)}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson.LastName.PadRight(17, ' ')}" +
                $"{new string(' ', 1)}" +
                $"{name}" +
                $"{new string(' ', 1)}" +
                $"{"1018Z981"}";
            Assert.NotNull(rowString);
            Assert.Equal(expectedRowString, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonAsStringRow_ValidPersonWithoutMiddleName_ReturnsExpectedTrimmedMiddleName()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(Gender.Male);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

            trsPerson.DateOfBirth = null;
            trsPerson.MiddleName = new string('c', 40);

            // Act
            var rowString = job.GetNewPersonAsStringRow(trsPerson, false);

            // Assert
            var name = $"{trsPerson.FirstName} {trsPerson.MiddleName}".Substring(0, 35); //Name is trimmed to 35 chars
            var expectedRowString = $"{trsPerson.Trn}" +
                $"{(int)Gender.Male}" +
                $"{new string(' ', 9)}" +
                $"{new string(' ', 6)}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson.LastName.PadRight(17, ' ')}" +
                $"{new string(' ', 1)}" +
                $"{name}" +
                $"{new string(' ', 1)}" +
                $"{"1018Z981"}";
            Assert.NotNull(rowString);
            Assert.Equal(expectedRowString, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithValidPreviousName_ReturnsExpectedContent(Gender gender)
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var newLastName = Faker.Name.Last();
            var originalastName = Faker.Name.Last();
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithGender(gender);
                x.WithLastName(originalastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            var oldPersonDetails = EventModels.PersonDetails.FromModel(trsPerson);
            trsPerson.LastName = newLastName;
            var newPersonDetails = EventModels.PersonDetails.FromModel(trsPerson);
            var nameChangeEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow.AddYears(-2),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = trsPerson.PersonId,
                PersonAttributes = newPersonDetails,
                OldPersonAttributes = oldPersonDetails,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = await job.GetNewPersonWithPreviousLastNameAsStringRowAsync(trsPerson, CancellationToken.None);

            // Assert
            var expectedRow = $"{trsPerson.Trn}" +
                $"{(int)gender}" +
                $"{new string(' ', 9)}" +
                $"{originalastName.PadRight(54, ' ')}" +
                $"{new string(' ', 7)}" +
                $"{"2018Z981"}";
            Assert.Equal(expectedRow, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithoutGender_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var newLastName = Faker.Name.Last();
            var originalastName = Faker.Name.Last();
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(originalastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            var oldPersonDetails = EventModels.PersonDetails.FromModel(trsPerson);
            trsPerson.LastName = newLastName;
            var newPersonDetails = EventModels.PersonDetails.FromModel(trsPerson);
            var nameChangeEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow.AddYears(-2),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = trsPerson.PersonId,
                PersonAttributes = newPersonDetails,
                OldPersonAttributes = oldPersonDetails,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = await job.GetNewPersonWithPreviousLastNameAsStringRowAsync(trsPerson, CancellationToken.None);

            // Assert
            var expectedRow = $"{trsPerson.Trn}" +
                $"{new string(' ', 1)}" +
                $"{new string(' ', 9)}" +
                $"{originalastName.PadRight(54, ' ')}" +
                $"{new string(' ', 7)}" +
                $"{"2018Z981"}";
            Assert.Equal(expectedRow, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithLastNameExceedingMaxLength_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var newLastName = new string('x', 60);
            var originalastName = new string('a', 75);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(originalastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            var oldPersonDetails = EventModels.PersonDetails.FromModel(trsPerson);
            trsPerson.LastName = newLastName;
            var newPersonDetails = EventModels.PersonDetails.FromModel(trsPerson);
            var nameChangeEvent = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow.AddYears(-2),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = trsPerson.PersonId,
                PersonAttributes = newPersonDetails,
                OldPersonAttributes = oldPersonDetails,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = await job.GetNewPersonWithPreviousLastNameAsStringRowAsync(trsPerson, CancellationToken.None);

            // Assert
            var expectedRow = $"{trsPerson.Trn}" +
                $"{new string(' ', 1)}" +
                $"{new string(' ', 9)}" +
                $"{person1.LastName.Substring(0, 54)}" +
                $"{new string(' ', 7)}" +
                $"{"2018Z981"}";
            Assert.Equal(expectedRow, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Fact]
    public async Task GetNewPersonWithPreviousLastNameAsStringRow_WithMultipleLastNameChanges_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var updateLastName1 = Faker.Name.Last();
            var updateLastName2 = Faker.Name.Last();
            var updateLastName3 = Faker.Name.Last();
            var originalastName = new string('a', 75);
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(originalastName);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);

            // first marriage
            var oldPersonDetails1 = EventModels.PersonDetails.FromModel(trsPerson);
            trsPerson.LastName = updateLastName1;
            var newPersonDetails1 = EventModels.PersonDetails.FromModel(trsPerson);
            var nameChangeEvent1 = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow.AddYears(-3),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = trsPerson.PersonId,
                PersonAttributes = newPersonDetails1,
                OldPersonAttributes = oldPersonDetails1,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent1);

            // second marriage
            var oldPersonDetails2 = EventModels.PersonDetails.FromModel(trsPerson);
            trsPerson.LastName = updateLastName2;
            var newPersonDetails2 = EventModels.PersonDetails.FromModel(trsPerson);
            var nameChangeEvent2 = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow.AddYears(-2),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = trsPerson.PersonId,
                PersonAttributes = newPersonDetails2,
                OldPersonAttributes = oldPersonDetails2,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent2);

            // third marriage
            var oldPersonDetails3 = EventModels.PersonDetails.FromModel(trsPerson);
            trsPerson.LastName = updateLastName3;
            var newPersonDetails3 = EventModels.PersonDetails.FromModel(trsPerson);
            var nameChangeEvent3 = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = trsPerson.PersonId,
                PersonAttributes = newPersonDetails3,
                OldPersonAttributes = oldPersonDetails3,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent3);
            await dbContext.SaveChangesAsync();

            // Act
            var rowString = await job.GetNewPersonWithPreviousLastNameAsStringRowAsync(trsPerson, CancellationToken.None);

            // Assert
            var expectedRow = $"{trsPerson.Trn}" +
                $"{new string(' ', 1)}" +
                $"{new string(' ', 9)}" +
                $"{updateLastName2.PadRight(54, ' ')}" +
                $"{new string(' ', 7)}" +
                $"{"2018Z981"}";
            Assert.Equal(expectedRow, rowString);
            Assert.Equal(ExpectedRowLength, rowString.Length);
        });
    }

    [Fact]
    public async Task Execute_NewRecordWithNoPreviousName_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            await dbContext.IntegrationTransactionRecords.ExecuteDeleteAsync();
            var expectedTotalRowCount = 1;
            var expectedTotalSuccessCount = 1;
            var expectedFailureCount = 0;
            var expectedDuplicateCount = 0;
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportNewJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-2).AddHours(-2).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            await dbContext.SaveChangesAsync();
            var originalastName = Faker.Name.Last();
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(originalastName);
                x.WithGender(Gender.Male);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            await dbContext.SaveChangesAsync();

            // Act
            var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var integrationTransaction = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionJobId);
            var expectedRowContent = $"{trsPerson.Trn}" +
                $"{(int)trsPerson.Gender!}" +
                $"{new string(' ', 9)}" +
                $"{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson.LastName.PadRight(17)}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35) +
                $"{new string(' ', 1)}" +
                $"{"1018Z981"}";
            Assert.NotNull(integrationTransaction);
            Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
            Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
            Assert.Equal(expectedTotalSuccessCount, integrationTransaction.SuccessCount);
            Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
            Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, record =>
            {
                Assert.Empty(record.FailureMessage!);
                Assert.Equal(person1.PersonId, record.PersonId);
                Assert.Null(record.Duplicate);
                Assert.NotNull(record.RowData);
                Assert.Equal(ExpectedRowLength, record.RowData!.Length);
                Assert.Equal(expectedRowContent, record.RowData);
                Assert.Equal(IntegrationTransactionRecordStatus.Success, record.Status);

                return true;
            });
        });
    }

    [Fact]
    public async Task Execute_WithNoNewRecords_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var expectedTotalRowCount = 0;
            var expectedTotalSuccessCount = 0;
            var expectedFailureCount = 0;
            var expectedDuplicateCount = 0;
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportNewJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-1).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            var originalastName = Faker.Name.Last();
            await dbContext.SaveChangesAsync();
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);

            // Act
            var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var integrationTransaction = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionJobId);
            Assert.NotNull(integrationTransaction);
            Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
            Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
            Assert.Equal(expectedTotalSuccessCount, integrationTransaction.SuccessCount);
            Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
            Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Empty(integrationTransaction.IntegrationTransactionRecords!);
        });
    }

    [Fact]
    public async Task Execute_NewRecordWithPreviousName_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var expectedTotalRowCount = 2;
            var expectedTotalSuccessCount = 2;
            var expectedFailureCount = 0;
            var expectedDuplicateCount = 0;
            var updateLastName = Faker.Name.Last();
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportNewJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-2).ToString("s", System.Globalization.CultureInfo.InvariantCulture)
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            await dbContext.SaveChangesAsync();
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var originalastName = Faker.Name.Last();
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(originalastName);
                x.WithGender(Gender.Male);
            });
            var trsPerson = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            var oldPersonDetails = EventModels.PersonDetails.FromModel(trsPerson);
            trsPerson.LastName = updateLastName;
            var newPersonDetails = EventModels.PersonDetails.FromModel(trsPerson);
            var nameChangeEvent1 = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = trsPerson.PersonId,
                PersonAttributes = newPersonDetails,
                OldPersonAttributes = oldPersonDetails,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent1);
            await dbContext.SaveChangesAsync();

            // Act
            var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var integrationTransaction = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionJobId);
            var expectedNewRowContent = $"{trsPerson.Trn}{(int)trsPerson.Gender!}{new string(' ', 9)}{trsPerson.DateOfBirth!.Value.ToString("ddMMyy")}{new string(' ', 1)}{trsPerson.LastName.PadRight(17)}{new string('1', 1)}{$"{trsPerson.FirstName} {trsPerson.MiddleName}".PadRight(35)}{new string(' ', 1)}{"1018Z981"}";

            var expectedNameChangeRow = $"{trsPerson.Trn}" +
                $"{(int)trsPerson.Gender}" +
                $"{new string(' ', 9)}" +
                $"{originalastName.PadRight(54, ' ')}" +
                $"{new string(' ', 7)}" +
                $"{"2018Z981"}";

            Assert.NotNull(integrationTransaction);
            Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
            Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
            Assert.Equal(expectedTotalSuccessCount, integrationTransaction.SuccessCount);
            Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
            Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNewRowContent, trsPerson));
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNameChangeRow, trsPerson));
        });
    }

    [Fact]
    public async Task Execute_MultipleNewRecordsMultiplePreviousNames_ReturnsExpectedContent()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var expectedTotalRowCount = 4;
            var expectedTotalSuccessCount = 4;
            var expectedFailureCount = 0;
            var expectedDuplicateCount = 0;
            var jobMetaData = new JobMetadata()
            {
                JobName = nameof(CapitaExportNewJob),
                Metadata = new Dictionary<string, string>
                    {
                        {
                            "LastRunDate", Clock.UtcNow.AddDays(-5).AddHours(-2).ToString("o")
                        }
                    }
            };
            dbContext.JobMetadata.Add(jobMetaData);
            await dbContext.SaveChangesAsync();
            var originalastName1 = Faker.Name.Last();
            var updateLastName1 = Faker.Name.Last();
            var person1 = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(originalastName1);
                x.WithGender(Gender.Male);
            });
            var originalastName2 = Faker.Name.Last();
            var updateLastName2 = Faker.Name.Last();
            var person2 = await TestData.CreatePersonAsync(x =>
            {
                x.WithLastName(originalastName2);
                x.WithGender(Gender.Male);
            });
            var trsPerson1 = await dbContext.Persons.FirstAsync(x => x.PersonId == person1.PersonId);
            var trsPerson2 = await dbContext.Persons.FirstAsync(x => x.PersonId == person2.PersonId);
            var oldPersonDetails1 = EventModels.PersonDetails.FromModel(trsPerson1);
            trsPerson1.LastName = updateLastName1;
            var newPersonDetails1 = EventModels.PersonDetails.FromModel(trsPerson1);
            var nameChangeEvent1 = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person1.PersonId,
                PersonAttributes = newPersonDetails1,
                OldPersonAttributes = oldPersonDetails1,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };

            var oldPersonDetails2 = EventModels.PersonDetails.FromModel(trsPerson2);
            trsPerson2.LastName = updateLastName2;
            var newPersonDetails2 = EventModels.PersonDetails.FromModel(trsPerson2);
            var nameChangeEvent2 = new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person2.PersonId,
                PersonAttributes = newPersonDetails2,
                OldPersonAttributes = oldPersonDetails2,
                NameChangeReason = "",
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            };
            dbContext.AddEventWithoutBroadcast(nameChangeEvent1);
            dbContext.AddEventWithoutBroadcast(nameChangeEvent2);
            await dbContext.SaveChangesAsync();

            // Act
            var integrationTransactionJobId = await job.ExecuteAsync(CancellationToken.None);

            // Assert
            var integrationTransaction = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).SingleAsync(x => x.IntegrationTransactionId == integrationTransactionJobId);
            var expectedNewRowContent1 = $"{trsPerson1.Trn}" +
                $"{(int)trsPerson1.Gender!}" +
                $"{new string(' ', 9)}" +
                $"{trsPerson1.DateOfBirth!.Value.ToString("ddMMyy")}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson1.LastName.PadRight(17)}" +
                $"{new string('1', 1)}" +
                $"{trsPerson1.FirstName} {trsPerson1.MiddleName}".PadRight(35) +
                $"{new string(' ', 1)}" +
                $"{"1018Z981"}";

            var expectedNameChangeRow1 = $"{trsPerson1.Trn}" +
                $"{(int)trsPerson1.Gender}" +
                $"{new string(' ', 9)}" +
                $"{originalastName1.PadRight(54, ' ')}" +
                $"{new string(' ', 7)}" +
                $"{"2018Z981"}";

            var expectedNewRowContent2 = $"{trsPerson2.Trn}" +
                $"{(int)trsPerson2.Gender!}" +
                $"{new string(' ', 9)}" +
                $"{trsPerson2.DateOfBirth!.Value.ToString("ddMMyy")}" +
                $"{new string(' ', 1)}" +
                $"{trsPerson2.LastName.PadRight(17)}" +
                $"{new string('1', 1)}" +
                $"{trsPerson2.FirstName} {trsPerson2.MiddleName}".PadRight(35) +
                $"{new string(' ', 1)}" +
                $"{"1018Z981"}";

            var expectedNameChangeRow2 = $"{trsPerson2.Trn}" +
                $"{(int)trsPerson2.Gender}" +
                $"{new string(' ', 9)}" +
                $"{originalastName2.PadRight(54, ' ')}" +
                $"{new string(' ', 7)}" +
                $"{"2018Z981"}";

            Assert.NotNull(integrationTransaction);
            Assert.Equal(IntegrationTransactionImportStatus.Success, integrationTransaction.ImportStatus);
            Assert.Equal(expectedTotalRowCount, integrationTransaction.TotalCount);
            Assert.Equal(expectedTotalSuccessCount, integrationTransaction.SuccessCount);
            Assert.Equal(expectedFailureCount, integrationTransaction.FailureCount);
            Assert.Equal(expectedDuplicateCount, integrationTransaction.DuplicateCount);
            Assert.NotEmpty(integrationTransaction.FileName);
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNewRowContent1, trsPerson1));
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNameChangeRow1, trsPerson1));
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNewRowContent2, trsPerson2));
            Assert.Contains(integrationTransaction.IntegrationTransactionRecords!, r => MatchesExpectedRowData(r, expectedNameChangeRow2, trsPerson2));
        });
    }

    [Fact]
    public async Task GetFileName_ReturnsExpectedFileName()
    {
        await WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var job = new CapitaExportNewJob(CreateDataLakeServiceClientMock(), NullLogger<CapitaExportNewJob>.Instance, dbContext, Clock);
            var expectedFileName = $"Reg01_DTR_{Clock.UtcNow.ToString("yyyyMMdd")}_{Clock.UtcNow.ToString("HHmmss")}_New.txt"; ;

            // Act
            var fileName = job.GetFileName(Clock);

            // Assert
            Assert.Equal(expectedFileName, fileName);
        });
    }

    bool MatchesExpectedRowData(IntegrationTransactionRecord record, string expectedRowData, Person person) =>
        record.PersonId == person.PersonId &&
        record.FailureMessage == string.Empty &&
        record.Duplicate == null &&
        record.RowData != null &&
        record.RowData.Length == ExpectedRowLength &&
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
