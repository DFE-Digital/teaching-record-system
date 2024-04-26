using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Services.Establishments.Tps;

public class TpsEstablishmentRefresher(
    ITpsExtractStorageService tpsExtractStorageService,
    IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task ImportFile(string fileName, CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var truncateSql = "TRUNCATE TABLE tps_establishments";
        await using var command = new NpgsqlCommand(truncateSql, connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);

        using var writer = await connection.BeginBinaryImportAsync(
            $"""
            COPY
                tps_establishments (
                    tps_establishment_id,
                    la_code,
                    establishment_code,
                    employers_name,
                    school_gias_name,
                    school_closed_date
                )
            FROM
                STDIN (FORMAT BINARY)
            """);

        var stream = await tpsExtractStorageService.GetFile(fileName, cancellationToken);
        using var streamReader = new StreamReader(stream);
        using var csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = true });

        await foreach (var row in csvReader.GetRecordsAsync<TpsEstablishmentCsvRow>())
        {
            if (string.IsNullOrEmpty(row.LaCode) || !Regex.IsMatch(row.LaCode, @"^\d{3}$")
                || string.IsNullOrEmpty(row.EstablishmentCode) || !Regex.IsMatch(row.EstablishmentCode, @"^\d{4}$")
                || string.IsNullOrEmpty(row.EmployersName) ||
                (!string.IsNullOrEmpty(row.SchoolClosedDate) && !DateOnly.TryParseExact(row.SchoolClosedDate, "dd/MM/yyyy", out _)))
            {
                continue;
            }

            writer.StartRow();
            writer.Write(Guid.NewGuid(), NpgsqlDbType.Uuid);
            writer.Write(row.LaCode, NpgsqlDbType.Char);
            writer.Write(row.EstablishmentCode, NpgsqlDbType.Char);
            writer.Write(row.EmployersName, NpgsqlDbType.Varchar);
            writer.Write(row.SchoolGiasName, NpgsqlDbType.Varchar);
            writer.Write(!string.IsNullOrEmpty(row.SchoolClosedDate) ? DateOnly.ParseExact(row.SchoolClosedDate, "dd/MM/yyyy") : (DateOnly?)null, NpgsqlDbType.Date);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RefreshEstablishments(CancellationToken cancellationToken)
    {
        using var readDbContext = dbContextFactory.CreateDbContext();
        readDbContext.Database.SetCommandTimeout(300);
        using var writeDbContext = dbContextFactory.CreateDbContext();

        FormattableString querySql =
            $"""
            WITH unique_gias_establishments AS (
                SELECT
                    establishment_id,
                    la_code,
                    la_name,
                    establishment_number,
                    establishment_name,
                    establishment_type_code,
                    postcode
                FROM
                    (SELECT
                        establishment_id,
                        la_code,
                        la_name,
                        establishment_number,
                        establishment_name,
                        establishment_type_code,
                        postcode,
                        ROW_NUMBER() OVER (PARTITION BY la_code, establishment_number, CASE WHEN establishment_number IS NULL THEN postcode ELSE NULL END ORDER BY translate(establishment_status_code::text, '1234', '1324'), urn desc) as row_number
                    FROM
                        establishments
                    WHERE
                        establishment_source_id = 1) e
                WHERE
                    e.row_number = 1
            ),
            gias_la_codes AS (
                SELECT
                    la_code,
                    MAX(la_name) as la_name
                FROM
                    unique_gias_establishments
                GROUP BY
                    la_code
            ),
            unique_tps_establishments AS (
                SELECT
                    tps_establishment_id,
                    la_code,
                    establishment_code,
                    employers_name,
                    school_gias_name,
                    school_closed_date
                FROM
                    (SELECT
                        tps_establishment_id,
                        la_code,
                        establishment_code,
                        employers_name,
                        school_gias_name,
                        school_closed_date,
                        ROW_NUMBER() OVER (PARTITION BY la_code, establishment_code ORDER BY CASE WHEN school_closed_date IS NULL THEN 1 ELSE 2 END) as row_number
                     FROM
                        tps_establishments) e
                WHERE
                    e.row_number = 1
            ),
            tps_la_names AS (
            	SELECT
            		*
            	FROM
            		(VALUES
            		('CORPORATION OF LONDON'),
            		('HAMMERSMITH & FULHAM'),
            		('KENSINGTON & CHELSEA'),
            		('BARKING & DAGENHAM'),
            		('KINGSTON-UPON-THAMES'),
            		('RICHMOND-UPON-THAMES'),
            		('ST HELENS'),
            		('NEWCASTLE-UPON-TYNE'),
            		('INNER LONDON'),
            		('ANGLESEY'),
            		('CARDIGANSHIRE'),
            		('NEATH & PORT TALBOT'),
            		('RHONDDA CYNON TAFF'),
            		('BATH & NORTH EAST SOMERSET'),
            		('CITY OF BRISTOL'),
            		('REDCAR & CLEVELAND'),
            		('CITY OF KINGSTON UPON HULL'),
            		('BEDFORD BOROUGH'),
            		('CITY OF DERBY'),
            		('BRIGHTON & HOVE'),
            		('LEICESTER CITY'),
            		('STOKE ON TRENT'),
            		('HEREFORDSHIRE'),
            		('MEDWAY TOWNS'),
            		('NOTTINGHAM CITY'),
            		('THE WREKIN'),
            		('BEDFORDSHIRE'),
            		('BERKSHIRE'),
            		('CHESHIRE'),
            		('CUMBRIA'),
            		('DURHAM'),
            		('HEREFORD & WORCESTER')) as t (name)
            )
            SELECT
                e.la_code,
                CASE WHEN g.la_code IS NULL THEN NULL ELSE g.la_name END as la_name,
                e.establishment_code as establishment_number,
                CASE WHEN t.short_description IS NULL OR (UPPER(g.la_name) != trim(e.employers_name) AND l.name IS NULL) THEN trim(e.employers_name) ELSE t.short_description END as establishment_name
            FROM
                    unique_tps_establishments e
                LEFT JOIN
                    tps_establishment_types t ON e.establishment_code::int >= t.establishment_range_from::int
                                                 AND e.establishment_code::int <= t.establishment_range_to::int
                LEFT JOIN
                    gias_la_codes g ON e.la_code = g.la_code
                LEFT JOIN
                    tps_la_names l ON trim(e.employers_name) = l.name
            WHERE
                NOT EXISTS (SELECT
                                1
                            FROM
                                unique_gias_establishments g
                            WHERE
                                g.la_code = e.la_code
                                AND g.establishment_number = e.establishment_code)
            """;

        int i = 0;
        await foreach (var item in readDbContext.Database.SqlQuery<NewEstablishment>(querySql).AsAsyncEnumerable())
        {
            var existingEstablishment = await writeDbContext.Establishments.SingleOrDefaultAsync(e => e.LaCode == item.LaCode && e.EstablishmentNumber == item.EstablishmentNumber);
            if (existingEstablishment == null)
            {
                writeDbContext.Establishments.Add(new()
                {
                    EstablishmentId = Guid.NewGuid(),
                    EstablishmentSourceId = 2,
                    Urn = null,
                    LaCode = item.LaCode,
                    LaName = item.LaName,
                    EstablishmentNumber = item.EstablishmentNumber,
                    EstablishmentName = item.EstablishmentName,
                    EstablishmentTypeCode = null,
                    EstablishmentTypeName = null,
                    EstablishmentTypeGroupCode = null,
                    EstablishmentTypeGroupName = null,
                    EstablishmentStatusCode = null,
                    EstablishmentStatusName = null,
                    Street = null,
                    Locality = null,
                    Address3 = null,
                    Town = null,
                    County = null,
                    Postcode = null
                });
            }
            else
            {
                existingEstablishment.EstablishmentSourceId = 2;
                existingEstablishment.LaName = item.LaName;
                existingEstablishment.EstablishmentName = item.EstablishmentName;
            }

            if (++i % 2000 == 0)
            {
                await writeDbContext.SaveChangesAsync(cancellationToken);
            }
        }

        if (writeDbContext.ChangeTracker.HasChanges())
        {
            await writeDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
