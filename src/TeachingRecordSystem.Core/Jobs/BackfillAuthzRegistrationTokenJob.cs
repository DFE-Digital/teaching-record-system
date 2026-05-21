using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;

#pragma warning disable DAP005

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillAuthzRegistrationTokenJob(TrsDbContext trsDbContext, IConfiguration configuration)
{
    public const string JobSchedule = "0 * * * *";

    private const int BatchSize = 500;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var sourceConnectionString =
            configuration.GetConnectionString("ID") ??
            throw new InvalidOperationException(
                "Connection string 'ID' was not found.");

        await using var sourceConnection =
            new NpgsqlConnection(sourceConnectionString);

        await sourceConnection.OpenAsync(cancellationToken);

        var targetConnection =
            (NpgsqlConnection)trsDbContext.Database.GetDbConnection();

        if (targetConnection.State != ConnectionState.Open)
        {
            await targetConnection.OpenAsync(cancellationToken);
        }

        var offset = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();


            var batch = (await sourceConnection.QueryAsync<SourceTrnToken>(
                """
                SELECT
                    trn_token AS TrnToken,
                    trn,
                    email,
                    created_utc AS CreatedUtc,
                    expires_utc AS ExpiresUtc,
                    user_id AS UserId
                FROM public.trn_tokens
                ORDER BY created_utc
                LIMIT @BatchSize
                OFFSET @Offset
                """,
                new
                {
                    BatchSize,
                    Offset = offset
                }))
                .ToList();

            if (batch.Count == 0)
            {
                break;
            }

            await using var transaction =
                await targetConnection.BeginTransactionAsync(cancellationToken);

            await targetConnection.ExecuteAsync(
                """
                CREATE TEMP TABLE tmp_authz_registration_tokens
                (
                    token varchar(128),
                    trn character(7),
                    emailaddress varchar(200),
                    created_utc timestamptz,
                    expires_utc timestamptz,
                    is_active boolean
                )
                ON COMMIT DROP;
                """,
                transaction: transaction);

            await using (var importer =
                await targetConnection.BeginBinaryImportAsync(
                    """
                    COPY tmp_authz_registration_tokens
                    (
                        token,
                        trn,
                        emailaddress,
                        created_utc,
                        expires_utc,
                        is_active
                    )
                    FROM STDIN (FORMAT BINARY)
                    """,
                    cancellationToken))
            {
                foreach (var row in batch)
                {
                    await importer.StartRowAsync(cancellationToken);

                    await importer.WriteAsync(
                        row.TrnToken,
                        NpgsqlDbType.Varchar,
                        cancellationToken);

                    await importer.WriteAsync(
                        row.Trn,
                        NpgsqlDbType.Char,
                        cancellationToken);

                    await importer.WriteAsync(
                        row.Email,
                        NpgsqlDbType.Varchar,
                        cancellationToken);

                    await importer.WriteAsync(
                        row.CreatedUtc,
                        NpgsqlDbType.TimestampTz,
                        cancellationToken);

                    await importer.WriteAsync(
                        row.ExpiresUtc,
                        NpgsqlDbType.TimestampTz,
                        cancellationToken);

                    await importer.WriteAsync(
                        row.UserId is null,
                        NpgsqlDbType.Boolean,
                        cancellationToken);
                }

                await importer.CompleteAsync(cancellationToken);
            }

            await targetConnection.ExecuteAsync(
                """
                MERGE INTO public.authz_registration_tokens AS target
                USING tmp_authz_registration_tokens AS source
                ON target.token =
                    source.token

                WHEN NOT MATCHED THEN
                    INSERT
                    (
                        token,
                        trn,
                        emailaddress,
                        created_utc,
                        expires_utc,
                        is_active
                    )
                    VALUES
                    (
                        source.token,
                        source.trn,
                        source.emailaddress,
                        source.created_utc,
                        source.expires_utc,
                        source.is_active
                    );
                """,
                transaction: transaction);

            await transaction.CommitAsync(cancellationToken);

            offset += BatchSize;
        }
    }

    private sealed class SourceTrnToken
    {
        public required string TrnToken { get; init; }

        public required string Trn { get; init; }

        public required string Email { get; init; }

        public required DateTime CreatedUtc { get; init; }

        public required DateTime ExpiresUtc { get; init; }

        public required Guid? UserId { get; init; }
    }
}

#pragma warning restore DAP005
