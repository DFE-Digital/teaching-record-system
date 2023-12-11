using Microsoft.Data.SqlClient;
using Microsoft.Xrm.Sdk;
using Xunit.Sdk;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.Services.DqtReporting;

public class DqtReportingServiceTests : IClassFixture<DqtReportingFixture>
{
    private readonly DqtReportingFixture _fixture;

    public DqtReportingServiceTests(DqtReportingFixture fixture)
    {
        _fixture = fixture;
    }

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
        await _fixture.PublishChangedItemsAndConsume(newItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.NotNull(row);
        Assert.Equal(newContact.Id, row["Id"]);
        Assert.Equal(newContact.FirstName, row["firstname"]);
        Assert.Equal(newContact.LastName, row["lastname"]);
        Assert.Equal(_fixture.Clock.UtcNow, row["__Inserted"]);
        Assert.Equal(_fixture.Clock.UtcNow, row["__Updated"]);
    }

    [Fact]
    public async Task ProcessChangesForEntityType_WritesUpdatedRecordToDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var insertedTime = _fixture.Clock.UtcNow.AddDays(-10);

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
        await _fixture.PublishChangedItemsAndConsume(newItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.NotNull(row);
        Assert.Equal(updatedContact.Id, row["Id"]);
        Assert.Equal(updatedContact.FirstName, row["firstname"]);
        Assert.Equal(updatedContact.LastName, row["lastname"]);
        Assert.Equal(insertedTime, row["__Inserted"]);
        Assert.Equal(_fixture.Clock.UtcNow, row["__Updated"]);
    }

    [Fact]
    public async Task ProcessChangesForEntityType_DeletesRemovedRecordFromDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var insertedTime = _fixture.Clock.UtcNow.AddDays(-10);

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
        await _fixture.PublishChangedItemsAndConsume(removedItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.Null(row);
        await AssertInDeleteLog(Contact.EntityLogicalName, contactId, expectedDeleted: _fixture.Clock.UtcNow);
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
                { "__Inserted", _fixture.Clock.UtcNow.Subtract(TimeSpan.FromHours(1)) }
            });
        }

        var contact1 = new Contact()
        {
            Id = contactId,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            ModifiedOn = _fixture.Clock.UtcNow,
        };

        var contact2 = new Contact()
        {
            Id = contactId,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            ModifiedOn = _fixture.Clock.UtcNow.AddMinutes(1),
        };

        var newItem1 = new NewOrUpdatedItem(ChangeType.NewOrUpdated, contact1);
        var newItem2 = new NewOrUpdatedItem(ChangeType.NewOrUpdated, contact2);

        // Act
        await _fixture.PublishChangedItemsAndConsume(newItem1, newItem2);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.NotNull(row);
        Assert.Equal(contactId, row["Id"]);
        Assert.Equal(contact2.FirstName, row["firstname"]);
        Assert.Equal(contact2.LastName, row["lastname"]);
        Assert.Equal(_fixture.Clock.UtcNow, row["__Updated"]);
    }

    private async Task AssertInDeleteLog(string entityLogicalName, Guid entityId, DateTime expectedDeleted)
    {
        using var sqlConnection = new SqlConnection(_fixture.ReportingDbConnectionString);
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
        Assert.Equal(expectedDeleted, deleted);
    }

    private async Task<IReadOnlyDictionary<string, object?>?> GetRowById(string tableName, Guid id)
    {
        using var sqlConnection = new SqlConnection(_fixture.ReportingDbConnectionString);
        await sqlConnection.OpenAsync();

        var cmd = new SqlCommand($"select * from {tableName} where id = @id");
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
        using var sqlConnection = new SqlConnection(_fixture.ReportingDbConnectionString);
        await sqlConnection.OpenAsync();

        var cmd = new SqlCommand(
            $"insert into {tableName} ({string.Join(", ", columns.Keys)}) values ({string.Join(", ", columns.Keys.Select((_, i) => $"@p{i}"))})");
        cmd.Connection = sqlConnection;

        var columnValues = columns.Values.ToArray();
        for (var i = 0; i < columns.Count; i++)
        {
            cmd.Parameters.Add(new SqlParameter($"@p{i}", columnValues[i]));
        }

        await cmd.ExecuteNonQueryAsync();
    }
}
