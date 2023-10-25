using Microsoft.EntityFrameworkCore;
using Microsoft.Xrm.Sdk;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Dqt.Services.CrmDataSync;

public class CrmDataSyncHelper
{
    private const string NowParameterName = "@now";

    private static readonly EntitySyncSql _contactSyncSql = GetEntitySyncSqlForContact();

    private readonly IDbContextFactory<TrsDbContext> _dbContextFactory;
    private readonly IClock _clock;

    public CrmDataSyncHelper(IDbContextFactory<TrsDbContext> dbContextFactory, IClock clock)
    {
        _dbContextFactory = dbContextFactory;
        _clock = clock;
    }

    public Task SyncContact(Contact entity, CancellationToken cancellationToken = default) =>
        SyncContacts(new[] { entity }, cancellationToken);

    public async Task SyncContacts(IReadOnlyCollection<Contact> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            CheckForRequiredAttributes(
                entity,
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
                Contact.Fields.EMailAddress1);
        }

        // For now, only sync records that have TRNs
        var toSync = entities.Where(e => !string.IsNullOrEmpty(e.dfeta_TRN)).ToArray();

        if (toSync.Length == 0)
        {
            return;
        }

        var people = toSync.Select(e => MapContact(e));

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = _contactSyncSql.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(_contactSyncSql.CopyStatement, cancellationToken);

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

        using (var mergeCommand = connection.CreateCommand())
        {
            mergeCommand.CommandText = _contactSyncSql.InsertStatement;
            mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, _clock.UtcNow));
            mergeCommand.Transaction = txn;
            await mergeCommand.ExecuteNonQueryAsync();
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

    private static EntitySyncSql GetEntitySyncSqlForContact()
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

        return new(createTempTableStatement, copyStatement, mergeStatement);
    }

    private static void CheckForRequiredAttributes(Entity entity, params string[] requiredAttributeNames)
    {
        foreach (var attribute in requiredAttributeNames)
        {
            if (!entity.Attributes.ContainsKey(attribute))
            {
                throw new ArgumentException($"Entity '{entity.Id}' is missing the '{attribute}' attribute.", nameof(entity));
            }
        }
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

    private record EntitySyncSql(string CreateTempTableStatement, string CopyStatement, string InsertStatement);
}

file static class Extensions
{
    /// <summary>
    /// Returns <c>null</c> if <paramref name="value"/> is empty or whitespace.
    /// </summary>
    public static string? NormalizeString(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
