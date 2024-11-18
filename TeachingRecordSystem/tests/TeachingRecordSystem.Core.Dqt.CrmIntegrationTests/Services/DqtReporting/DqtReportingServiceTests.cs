using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.DqtReporting;
using Xunit.Sdk;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.Services.DqtReporting;

[Collection(nameof(ExclusiveCrmTestCollection))]
public class DqtReportingServiceTests(DqtReportingFixture fixture) : IClassFixture<DqtReportingFixture>
{
    private static readonly TimeSpan _dateTimeComparisonTolerance = TimeSpan.FromMilliseconds(500);

    [Fact]
    public async Task ProcessChangesForEntityType_WritesNewRecordToDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var newContact = new Contact()
        {
            Id = contactId,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
        };

        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, newContact);

        // Act
        await fixture.PublishChangedItemsAndConsume(newItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.NotNull(row);
        Assert.Equal(newContact.Id, row["Id"]);
        Assert.Equal(newContact.FirstName, row["firstname"]);
        Assert.Equal(newContact.LastName, row["lastname"]);
        Assert.Equal(fixture.Clock.UtcNow, (DateTime)row["__Inserted"]!, _dateTimeComparisonTolerance);
        Assert.Equal(fixture.Clock.UtcNow, (DateTime)row["__Updated"]!, _dateTimeComparisonTolerance);
    }

    [Fact]
    public async Task ProcessChangesForEntityType_WritesUpdatedRecordToDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var insertedTime = fixture.Clock.UtcNow.AddDays(-10);

        await InsertRow(Contact.EntityLogicalName, new Dictionary<string, object?>()
        {
            { "Id", contactId },
            { "firstname", Faker.Name.First() },
            { "lastname", Faker.Name.Last() },
            { "__Inserted", insertedTime }
        });

        var updatedContact = new Contact()
        {
            Id = contactId,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
        };

        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, updatedContact);

        // Act
        await fixture.PublishChangedItemsAndConsume(newItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.NotNull(row);
        Assert.Equal(updatedContact.Id, row["Id"]);
        Assert.Equal(updatedContact.FirstName, row["firstname"]);
        Assert.Equal(updatedContact.LastName, row["lastname"]);
        Assert.Equal(insertedTime, (DateTime)row["__Inserted"]!, _dateTimeComparisonTolerance);
        Assert.Equal(fixture.Clock.UtcNow, (DateTime)row["__Updated"]!, _dateTimeComparisonTolerance);
    }

    [Fact]
    public async Task ProcessChangesForEntityType_DeletesRemovedRecordFromDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var insertedTime = fixture.Clock.UtcNow.AddDays(-10);

        await InsertRow(Contact.EntityLogicalName, new Dictionary<string, object?>()
        {
            { "Id", contactId },
            { "firstname", Faker.Name.First() },
            { "lastname", Faker.Name.Last() },
            { "__Inserted", insertedTime }
        });

        var removedItem = new RemovedOrDeletedItem(
            ChangeType.RemoveOrDeleted,
            new EntityReference(Contact.EntityLogicalName, contactId));

        // Act
        await fixture.PublishChangedItemsAndConsume(removedItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.Null(row);
        await AssertInDeleteLog(Contact.EntityLogicalName, contactId, expectedDeleted: fixture.Clock.UtcNow);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProcessChangesForEntityType_SameRecordMultipleTimesInBatch_WritesMostRecentUpdate(bool contactExistsPreSync)
    {
        // Arrange
        var contactId = Guid.NewGuid();

        if (contactExistsPreSync)
        {
            await InsertRow(Contact.EntityLogicalName, new Dictionary<string, object?>()
            {
                { "Id", contactId },
                { "firstname", Faker.Name.First() },
                { "lastname", Faker.Name.Last() },
                { "__Inserted", fixture.Clock.UtcNow.Subtract(TimeSpan.FromHours(1)) }
            });
        }

        var contact1 = new Contact()
        {
            Id = contactId,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            ModifiedOn = fixture.Clock.UtcNow,
        };

        var contact2 = new Contact()
        {
            Id = contactId,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            ModifiedOn = fixture.Clock.UtcNow.AddMinutes(1),
        };

        var newItem1 = new NewOrUpdatedItem(ChangeType.NewOrUpdated, contact1);
        var newItem2 = new NewOrUpdatedItem(ChangeType.NewOrUpdated, contact2);

        // Act
        await fixture.PublishChangedItemsAndConsume(newItem1, newItem2);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.NotNull(row);
        Assert.Equal(contactId, row["Id"]);
        Assert.Equal(contact2.FirstName, row["firstname"]);
        Assert.Equal(contact2.LastName, row["lastname"]);
        Assert.Equal(fixture.Clock.UtcNow, (DateTime)row["__Updated"]!, _dateTimeComparisonTolerance);
    }

    [Fact(Skip = "Flaky on CI")]
    public Task ProcessTrsChanges_NewRow_IsInsertedIntoDbWithCorrectValues() => ProcessTrsChangesSingle(async singleMessageConsumed =>
    {
        // Arrange
        await using var testDataScope = fixture.CreateTestDataScope(withSync: true);

        var mqProviderId = MandatoryQualificationProvider.All.First().MandatoryQualificationProviderId;
        var mqSpecialism = MandatoryQualificationSpecialism.MultiSensory;
        var mqStatus = MandatoryQualificationStatus.Passed;
        var mqStartDate = new DateOnly(2022, 5, 1);
        var mqEndDate = new DateOnly(2023, 4, 1);

        // Act
        var createPersonResult = await testDataScope.TestData.CreatePersonAsync(p => p
            .WithMandatoryQualification(m => m
                .WithProvider(mqProviderId)
                .WithSpecialism(mqSpecialism)
                .WithStartDate(mqStartDate)
                .WithStatus(mqStatus, mqEndDate)));
        var mq = createPersonResult.MandatoryQualifications.Single();

        await singleMessageConsumed;

        // Assert
        var row = await GetRowById("trs_qualifications", mq.QualificationId, idColumnName: "qualification_id");
        Assert.NotNull(row);
        Assert.Equal(createPersonResult.PersonId, row["person_id"]);
        Assert.Equal((int)QualificationType.MandatoryQualification, row["qualification_type"]);
        Assert.Equal(mqProviderId, row["mq_provider_id"]);
        Assert.Equal((int)mqSpecialism, row["mq_specialism"]);
        Assert.Equal((int)mqStatus, row["mq_status"]);
        Assert.Equal(mqStartDate, ConvertDateTimeColumn(row["start_date"]));
        Assert.Equal(mqEndDate, ConvertDateTimeColumn(row["end_date"]));
        Assert.Equal(fixture.Clock.UtcNow, (DateTime)row["__Inserted"]!, _dateTimeComparisonTolerance);
        Assert.Equal(fixture.Clock.UtcNow, (DateTime)row["__Updated"]!, _dateTimeComparisonTolerance);

        static DateOnly? ConvertDateTimeColumn(object? value) => value is null ? null : DateOnly.FromDateTime((DateTime)value);
    });

    [Fact(Skip = "Flaky on CI")]
    public async Task ProcessTrsChanges_ExistingRow_IsUpdatedInDbWithCorrectValues()
    {
        // Arrange
        await using var testDataScope = fixture.CreateTestDataScope(withSync: true);

        var insertedTime = fixture.Clock.UtcNow.AddDays(-10);

        var mqProviderId = MandatoryQualificationProvider.All.First().MandatoryQualificationProviderId;
        var mqSpecialism = MandatoryQualificationSpecialism.MultiSensory;
        var mqStatus = MandatoryQualificationStatus.Passed;
        var mqStartDate = new DateOnly(2022, 5, 1);
        var mqEndDate = new DateOnly(2023, 4, 1);

        var createPersonResult = await testDataScope.TestData.CreatePersonAsync(p => p.WithMandatoryQualification());
        var mq = createPersonResult.MandatoryQualifications.Single();

        await InsertRow("trs_qualifications", new Dictionary<string, object?>()
        {
            { "qualification_id", mq.QualificationId },
            { "person_id", createPersonResult.PersonId },
            { "qualification_type", (int)QualificationType.MandatoryQualification },
            { "mq_provider_id", mq.ProviderId },
            { "mq_specialism", mq.Specialism },
            { "mq_status", mq.Status },
            { "start_date", mq.StartDate },
            { "end_date", mq.EndDate },
            { "__Inserted", insertedTime }
        });

        await ProcessTrsChangesSingle(async singleMessageConsumed =>
        {
            // Act
            await fixture.DbFixture.WithDbContextAsync(async dbContext =>
            {
                var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.QualificationId == mq.QualificationId);
                qualification.ProviderId = mqProviderId;
                qualification.Specialism = mqSpecialism;
                qualification.Status = mqStatus;
                qualification.StartDate = mqStartDate;
                qualification.EndDate = mqEndDate;
                await dbContext.SaveChangesAsync();
            });

            await singleMessageConsumed;

            // Assert
            var row = await GetRowById("trs_qualifications", mq.QualificationId, idColumnName: "qualification_id");
            Assert.NotNull(row);
            Assert.Equal(createPersonResult.PersonId, row["person_id"]);
            Assert.Equal((int)QualificationType.MandatoryQualification, row["qualification_type"]);
            Assert.Equal(mqProviderId, row["mq_provider_id"]);
            Assert.Equal((int)mqSpecialism, row["mq_specialism"]);
            Assert.Equal((int)mqStatus, row["mq_status"]);
            Assert.Equal(mqStartDate, ConvertDateTimeColumn(row["start_date"]));
            Assert.Equal(mqEndDate, ConvertDateTimeColumn(row["end_date"]));
            Assert.Equal(fixture.Clock.UtcNow, (DateTime)row["__Updated"]!, _dateTimeComparisonTolerance);
        });

        static DateOnly? ConvertDateTimeColumn(object? value) => value is null ? null : DateOnly.FromDateTime((DateTime)value);
    }

    [Fact(Skip = "Flaky on CI")]
    public async Task ProcessTrsChanges_SourceRowIsDeleted_DeletesRowFromDb()
    {
        // Arrange
        await using var testDataScope = fixture.CreateTestDataScope(withSync: true);

        var insertedTime = fixture.Clock.UtcNow.AddDays(-10);

        var createPersonResult = await testDataScope.TestData.CreatePersonAsync(p => p.WithMandatoryQualification());
        var mq = createPersonResult.MandatoryQualifications.Single();

        await InsertRow("trs_qualifications", new Dictionary<string, object?>()
        {
            { "qualification_id", mq.QualificationId },
            { "person_id", createPersonResult.PersonId },
            { "qualification_type", (int)QualificationType.MandatoryQualification },
            { "mq_provider_id", mq.ProviderId },
            { "mq_specialism", mq.Specialism },
            { "mq_status", mq.Status },
            { "start_date", mq.StartDate },
            { "end_date", mq.EndDate },
            { "__Inserted", insertedTime }
        });

        await ProcessTrsChangesSingle(async singleMessageConsumed =>
        {
            // Act
            await fixture.DbFixture.WithDbContextAsync(async dbContext =>
            {
                await dbContext.MandatoryQualifications.Where(q => q.QualificationId == mq.QualificationId).ExecuteDeleteAsync();
            });

            await singleMessageConsumed;

            // Assert
            var row = await GetRowById("trs_qualifications", mq.QualificationId, idColumnName: "qualification_id");
            Assert.Null(row);
        });
    }

    [Fact(Skip = "Flaky on CI")]
    public async Task ProcessTrsChanges_SourceTableIsTruncated_DeletesRowsFromDb()
    {
        // Arrange
        await using var testDataScope = fixture.CreateTestDataScope(withSync: true);

        var insertedTime = fixture.Clock.UtcNow.AddDays(-10);

        var createPersonResult = await testDataScope.TestData.CreatePersonAsync(p => p.WithMandatoryQualification());
        var mq = createPersonResult.MandatoryQualifications.Single();

        await InsertRow("trs_qualifications", new Dictionary<string, object?>()
        {
            { "qualification_id", mq.QualificationId },
            { "person_id", createPersonResult.PersonId },
            { "qualification_type", (int)QualificationType.MandatoryQualification },
            { "mq_provider_id", mq.ProviderId },
            { "mq_specialism", mq.Specialism },
            { "mq_status", mq.Status },
            { "start_date", mq.StartDate },
            { "end_date", mq.EndDate },
            { "__Inserted", insertedTime }
        });

        await ProcessTrsChangesSingle(async singleMessageConsumed =>
        {
            // Act
            await fixture.DbFixture.WithDbContextAsync(async dbContext =>
            {
                await dbContext.Database.ExecuteSqlAsync($"truncate table qualifications");
            });

            await singleMessageConsumed;

            // Assert
            var row = await GetRowById("trs_qualifications", mq.QualificationId, idColumnName: "qualification_id");
            Assert.Null(row);
        });
    }

    private Task ProcessTrsChangesSingle(Func<Task, Task> action) => fixture.WithService(async (service, _) =>
    {
        try
        {
            // ProcessTrsChanges will call OnNext on any provided IObserver to signal when it's established replication or consumed a message;
            // this Subject provides that IObserver.
            var replicationStatusSubject = new System.Reactive.Subjects.ReplaySubject<DqtReportingService.TrsReplicationStatus>();

            using var cts = new CancellationTokenSource();

            var processChangesTask = Task.Run(() => service.ProcessTrsChangesAsync(observer: replicationStatusSubject, cancellationToken: cts.Token));

            // Wait until the replication slot has been established
            await replicationStatusSubject.FirstAsync(s => s == DqtReportingService.TrsReplicationStatus.ReplicationSlotEstablished).ToTask(cts.Token);

            // We need to pass a Task to the `action` delegate that it can await so that it knows when we've consumed a single message from the replication stream.
            // We also need to ensure any exceptions from ProcessTrsChanges are surfaced.
            // The CancellationToken ensures that we're not waiting forever.
            async Task ConsumeSingleMessage()
            {
                var waitForMessageConsumedTask = WaitForMessageConsumed();

                var firstCompletedTask = await Task.WhenAny(waitForMessageConsumedTask, processChangesTask);
                cts.Cancel();

                await firstCompletedTask;

                Task WaitForMessageConsumed() =>
                    replicationStatusSubject.FirstAsync(s => s == DqtReportingService.TrsReplicationStatus.MessageConsumed).ToTask(cts.Token);
            }

            cts.CancelAfter(20000);

            try
            {
                await action(ConsumeSingleMessage());
            }
            finally
            {
                await processChangesTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                cts.Cancel();
            }
        }
        finally
        {
            await fixture.DbFixture.DropReplicationSlotAsync(fixture.TrsDbReplicationSlotName);
        }
    });

    private async Task AssertInDeleteLog(string entityLogicalName, Guid entityId, DateTime expectedDeleted)
    {
        using var sqlConnection = new SqlConnection(fixture.ReportingDbConnectionString);
        await sqlConnection.OpenAsync();

        var cmd = new SqlCommand("select Deleted from [__DeleteLog] where EntityId = @EntityId and EntityType = @EntityType");
        cmd.Connection = sqlConnection;
        cmd.Parameters.Add(new SqlParameter("@EntityId", entityId));
        cmd.Parameters.Add(new SqlParameter("@EntityType", entityLogicalName));

        using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            throw new XunitException($"Entity is not in __DeleteLog.");
        }

        var deleted = reader.GetDateTime(0);
        Assert.Equal(expectedDeleted, deleted, _dateTimeComparisonTolerance);
    }

    private async Task<IReadOnlyDictionary<string, object?>?> GetRowById(string tableName, Guid id, string idColumnName = "id")
    {
        using var sqlConnection = new SqlConnection(fixture.ReportingDbConnectionString);
        await sqlConnection.OpenAsync();

        var cmd = new SqlCommand($"select * from {tableName} where {idColumnName} = @id");
        cmd.Connection = sqlConnection;
        cmd.Parameters.Add(new SqlParameter("@id", id));

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var data = new object[reader.FieldCount];
            reader.GetValues(data);

            return data
                .Select((value, index) => (Value: value == DBNull.Value ? null : value, FieldName: reader.GetName(index)))
                .ToDictionary(t => t.FieldName, t => t.Value);
        }

        return null;
    }

    private async Task InsertRow(string tableName, IReadOnlyDictionary<string, object?> columns)
    {
        using var sqlConnection = new SqlConnection(fixture.ReportingDbConnectionString);
        await sqlConnection.OpenAsync();

        var cmd = new SqlCommand(
            $"insert into {tableName} ({string.Join(", ", columns.Keys)}) values ({string.Join(", ", columns.Keys.Select((_, i) => $"@p{i}"))})");
        cmd.Connection = sqlConnection;

        var columnValues = columns.Values.ToArray();
        for (var i = 0; i < columns.Count; i++)
        {
            cmd.Parameters.Add(new SqlParameter($"@p{i}", columnValues[i] ?? DBNull.Value));
        }

        await cmd.ExecuteNonQueryAsync();
    }
}
