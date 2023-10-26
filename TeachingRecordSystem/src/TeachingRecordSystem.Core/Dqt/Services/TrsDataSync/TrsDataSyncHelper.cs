using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Xrm.Sdk;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Dqt.Services.TrsDataSync;

public class TrsDataSyncHelper
{
    private delegate Task SyncEntitiesHandler(IReadOnlyCollection<Entity> entities, bool ignoreInvalid, CancellationToken cancellationToken);

    private const string NowParameterName = "@now";
    private const string IdsParameterName = "@ids";

    private static readonly IReadOnlyDictionary<string, EntitySyncInfo> _entitySyncInfo = new Dictionary<string, EntitySyncInfo>()
    {
        { Contact.EntityLogicalName, GetEntitySyncInfoForContact() }
    };

    private readonly IDbContextFactory<TrsDbContext> _dbContextFactory;
    private readonly IClock _clock;

    public TrsDataSyncHelper(IDbContextFactory<TrsDbContext> dbContextFactory, IClock clock)
    {
        _dbContextFactory = dbContextFactory;
        _clock = clock;
    }

    public async Task DeleteEntities(string entityLogicalName, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        var entitySyncInfo = GetEntitySyncInfo(entityLogicalName);

        if (ids.Count == 0)
        {
            return;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = entitySyncInfo.DeleteStatement;
            cmd.Parameters.Add(new NpgsqlParameter(IdsParameterName, ids.ToArray()));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public string[] GetSyncedAttributeNames(string entityLogicalName)
    {
        var entitySyncInfo = GetEntitySyncInfo(entityLogicalName);
        return entitySyncInfo.AttributeNames;
    }

    public Task SyncEntities(string entityLogicalName, IReadOnlyCollection<Entity> entities, bool ignoreInvalid, CancellationToken cancellationToken = default)
    {
        var entitySyncInfo = GetEntitySyncInfo(entityLogicalName);
        return entitySyncInfo.GetSyncHandler(this)(entities, ignoreInvalid, cancellationToken);
    }

    public Task SyncContact(Contact entity, bool ignoreInvalid, CancellationToken cancellationToken = default) =>
        SyncContacts(new[] { entity }, ignoreInvalid, cancellationToken);

    public async Task SyncContacts(IReadOnlyCollection<Contact> entities, bool ignoreInvalid, CancellationToken cancellationToken = default)
    {
        var entitySyncInfo = GetEntitySyncInfo(Contact.EntityLogicalName);

        // For now, only sync records that have TRNs
        var toSync = entities.Where(e => !string.IsNullOrEmpty(e.dfeta_TRN));

        if (ignoreInvalid)
        {
            // Some bad data in the build environment has a TRN that's longer than 7 digits
            toSync = toSync.Where(e => e.dfeta_TRN.Length == 7);
        }

        var people = toSync.Select(e => MapContact(e)).ToArray();

        if (people.Length == 0)
        {
            return;
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = entitySyncInfo.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(entitySyncInfo.CopyStatement, cancellationToken);

        foreach (var person in people)
        {
            writer.StartRow();
            WriteValueOrNull(person.PersonId, NpgsqlDbType.Uuid);
            WriteValueOrNull(person.Trn, NpgsqlDbType.Char);
            WriteValueOrNull(person.FirstName, NpgsqlDbType.Varchar);
            WriteValueOrNull(person.MiddleName, NpgsqlDbType.Varchar);
            WriteValueOrNull(person.LastName, NpgsqlDbType.Varchar);
            WriteNullableValueOrNull(person.DateOfBirth, NpgsqlDbType.Date);
            WriteValueOrNull(person.EmailAddress, NpgsqlDbType.Varchar);
            WriteValueOrNull(person.NationalInsuranceNumber, NpgsqlDbType.Char);
            WriteNullableValueOrNull(person.DqtContactId, NpgsqlDbType.Uuid);
            WriteNullableValueOrNull(person.DqtState, NpgsqlDbType.Integer);
            WriteValueOrNull(person.DqtFirstName, NpgsqlDbType.Varchar);
            WriteValueOrNull(person.DqtMiddleName, NpgsqlDbType.Varchar);
            WriteValueOrNull(person.DqtLastName, NpgsqlDbType.Varchar);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        try
        {
            using (var mergeCommand = connection.CreateCommand())
            {
                mergeCommand.CommandText = entitySyncInfo.InsertStatement;
                mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, _clock.UtcNow));
                mergeCommand.Transaction = txn;
                await mergeCommand.ExecuteNonQueryAsync();
            }
        }
        catch (PostgresException ex) when (ignoreInvalid && ex.SqlState == "23505" && ex.ConstraintName == "ix_persons_trn")
        {
            // Record already exists with TRN.
            // Remove the record from the collection and try again.

            // ex.Detail will be something like "Key (trn)=(1000336) already exists."
            var trn = ex.Detail!.Substring("Key (trn)=(".Length, 7);
            Debug.Assert(trn.All(c => char.IsAsciiDigit(c)));

            await txn.DisposeAsync();
            await connection.DisposeAsync();
            await dbContext.DisposeAsync();

            await SyncContacts(entities.Where(e => e.dfeta_TRN != trn).ToArray(), ignoreInvalid, cancellationToken);

            return;
        }

        await txn.CommitAsync(cancellationToken);

        void WriteNullableValueOrNull<T>(Nullable<T> value, NpgsqlDbType dbType)
            where T : struct
        {
            if (value.HasValue)
            {
                writer.Write(value.Value, dbType);
            }
            else
            {
                writer.WriteNull();
            }
        }

        void WriteValueOrNull<T>(T? value, NpgsqlDbType dbType)
            where T : notnull
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.Write(value, dbType);
            }
        }
    }

    private static EntitySyncInfo GetEntitySyncInfo(string entityLogicalName) =>
        _entitySyncInfo.TryGetValue(entityLogicalName, out var entitySyncInfo) ?
            entitySyncInfo :
            throw new NotSupportedException($"Syncing {entityLogicalName} entities is not supported.");

    private static EntitySyncInfo GetEntitySyncInfoForContact()
    {
        var tempTableName = "temp_person_import";

        var columnNames = new[]
        {
            "person_id",
            "trn",
            "first_name",
            "middle_name",
            "last_name",
            "date_of_birth",
            "email_address",
            "national_insurance_number",
            "dqt_contact_id",
            "dqt_state",
            "dqt_first_name",
            "dqt_middle_name",
            "dqt_last_name"
        };

        var columnsToUpdate = columnNames.Except(new[] { "person_id", "dqt_contact_id" }).ToArray();

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"CREATE TEMP TABLE {tempTableName} (LIKE persons INCLUDING DEFAULTS)";

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var mergeStatement =
            $"""
            INSERT INTO persons ({columnList}, dqt_first_sync, dqt_last_sync)
            SELECT {columnList}, {NowParameterName}, {NowParameterName} FROM {tempTableName}
            ON CONFLICT (person_id) DO UPDATE SET dqt_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            """;

        var deleteStatement = $"DELETE FROM persons WHERE dqt_contact_id = ANY({IdsParameterName})";

        var attributeNames = new[]
        {
            Contact.Fields.ContactId,
            Contact.Fields.StateCode,
            Contact.Fields.dfeta_TRN,
            Contact.Fields.FirstName,
            Contact.Fields.MiddleName,
            Contact.Fields.LastName,
            Contact.Fields.dfeta_StatedFirstName,
            Contact.Fields.dfeta_StatedMiddleName,
            Contact.Fields.dfeta_StatedLastName,
            Contact.Fields.BirthDate,
            Contact.Fields.dfeta_NINumber,
            Contact.Fields.EMailAddress1
        };

        return new(
            createTempTableStatement,
            copyStatement,
            mergeStatement,
            deleteStatement,
            attributeNames,
            helper => (entities, ignoreInvalid, ct) => helper.SyncContacts(entities.Select(e => e.ToEntity<Contact>()).ToArray(), ignoreInvalid, ct));
    }

    private static Person MapContact(Contact contact) => new Person()
    {
        PersonId = contact.ContactId!.Value,
        Trn = contact.dfeta_TRN,
        FirstName = (contact.HasStatedNames() ? contact.dfeta_StatedFirstName : contact.FirstName) ?? string.Empty,
        MiddleName = (contact.HasStatedNames() ? contact.dfeta_StatedMiddleName : contact.MiddleName) ?? string.Empty,
        LastName = (contact.HasStatedNames() ? contact.dfeta_StatedLastName : contact.LastName) ?? string.Empty,
        DateOfBirth = contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
        EmailAddress = contact.EMailAddress1.NormalizeString(),
        NationalInsuranceNumber = contact.dfeta_NINumber.NormalizeString(),
        DqtContactId = contact.Id,
        DqtState = (int)contact.StateCode!,
        DqtFirstName = contact.FirstName ?? string.Empty,
        DqtMiddleName = contact.MiddleName ?? string.Empty,
        DqtLastName = contact.LastName ?? string.Empty
    };

    private record EntitySyncInfo(
        string CreateTempTableStatement,
        string CopyStatement,
        string InsertStatement,
        string DeleteStatement,
        string[] AttributeNames,
        Func<TrsDataSyncHelper, SyncEntitiesHandler> GetSyncHandler);
}

file static class Extensions
{
    /// <summary>
    /// Returns <c>null</c> if <paramref name="value"/> is empty or whitespace.
    /// </summary>
    public static string? NormalizeString(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
