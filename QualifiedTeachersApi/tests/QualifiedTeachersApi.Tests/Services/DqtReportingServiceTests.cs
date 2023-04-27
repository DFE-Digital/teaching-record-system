using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Xrm.Sdk;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.Services;

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
        newContact.Attributes.Add(Contact.PrimaryIdAttribute, contactId);

        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, newContact);

        // Act
        await _fixture.PublishChangedItemAndConsume(newItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.NotNull(row);
        Assert.Equal(newContact.Id, row["Id"]);
        Assert.Equal(newContact.FirstName, row["firstname"]);
        Assert.Equal(newContact.LastName, row["lastname"]);
    }

    [Fact]
    public async Task ProcessChangesForEntityType_WritesUpdatedRecordToDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();

        await InsertRow(Contact.EntityLogicalName, new Dictionary<string, object?>()
        {
            { "Id", contactId },
            { "firstname", Faker.Name.First() },
            { "lastname", Faker.Name.Last() }
        });

        var updatedContact = new Contact()
        {
            Id = contactId,
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
        };
        updatedContact.Attributes.Add(Contact.PrimaryIdAttribute, contactId);

        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, updatedContact);

        // Act
        await _fixture.PublishChangedItemAndConsume(newItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.NotNull(row);
        Assert.Equal(updatedContact.Id, row["Id"]);
        Assert.Equal(updatedContact.FirstName, row["firstname"]);
        Assert.Equal(updatedContact.LastName, row["lastname"]);
    }

    [Fact]
    public async Task ProcessChangesForEntityType_DeletesRemovedRecordFromDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();

        await InsertRow(Contact.EntityLogicalName, new Dictionary<string, object?>()
        {
            { "Id", contactId },
            { "firstname", Faker.Name.First() },
            { "lastname", Faker.Name.Last() }
        });

        var removedItem = new RemovedOrDeletedItem(
            ChangeType.RemoveOrDeleted,
            new EntityReference(Contact.EntityLogicalName, contactId));

        // Act
        await _fixture.PublishChangedItemAndConsume(removedItem);

        // Assert
        var row = await GetRowById(Contact.EntityLogicalName, contactId);
        Assert.Null(row);
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
