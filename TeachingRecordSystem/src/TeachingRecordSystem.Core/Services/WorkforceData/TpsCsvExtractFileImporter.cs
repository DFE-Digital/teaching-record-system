using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using Npgsql;
using NpgsqlTypes;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class TpsCsvExtractFileImporter(
    ITpsExtractStorageService tpsExtractStorageService,
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IClock clock)
{
    public async Task ImportFile(Guid tpsCsvExtractId, string fileName, CancellationToken cancellationToken)
    {
        var fileNameParts = fileName.Split("/");
        var fileNameWithoutFolder = fileNameParts.Last();

        using var dbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var insertTpsCsvExtractSql =
            $"""
            INSERT INTO tps_csv_extracts (
                tps_csv_extract_id,
                filename,
                created_on
            )
            VALUES (
                '{tpsCsvExtractId}',
                '{fileNameWithoutFolder}',
                '{clock.UtcNow}'
            )
            """;
        await using var command = new NpgsqlCommand(insertTpsCsvExtractSql, connection, transaction);
        await command.ExecuteNonQueryAsync(cancellationToken);

        using var writer = await connection.BeginBinaryImportAsync(
            $"""
            COPY
                tps_csv_extract_load_items (
                    tps_csv_extract_load_item_id,
                    tps_csv_extract_id,
                    trn,
                    national_insurance_number,
                    date_of_birth,
                    date_of_death,
                    member_postcode,
                    member_email_address,
                    local_authority_code,
                    establishment_number,
                    establishment_postcode,
                    establishment_email_address,
                    employment_start_date,
                    employment_end_date,
                    full_or_part_time_indicator,
                    withdrawl_indicator,
                    extract_date,
                    gender,                    
                    created,
                    errors
                )
            FROM
                STDIN (FORMAT BINARY)
            """);

        writer.Timeout = TimeSpan.FromMinutes(5);

        using var stream = await tpsExtractStorageService.GetFile(fileName, cancellationToken);
        using var streamReader = new StreamReader(stream);
        using var csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = true });

        var validGenderValues = new List<string>() { "Male", "Female" };
        var validFullOrPartTimeIndicatorValues = new List<string>() { "FT", "PTI", "PTR", "PT" };

        await foreach (var row in csvReader.GetRecordsAsync<TpsCsvExtractRowRaw>())
        {
            var loadErrors = TpsCsvExtractItemLoadErrors.None;
            if (row.Trn is null || !Regex.IsMatch(row.Trn, @"^\d{7}$"))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.TrnIncorrectFormat;
            }

            if (row.NationalInsuranceNumber is null || !NationalInsuranceNumberHelper.IsValid(row.NationalInsuranceNumber))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.NationalInsuranceNumberIncorrectFormat;
            }

            if (row.DateOfBirth is null || !DateOnly.TryParseExact(row.DateOfBirth, "dd/MM/yyyy", out _))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.DateOfBirthIncorrectFormat;
            }

            if (row.DateOfDeath is not null && !DateOnly.TryParseExact(row.DateOfDeath, "dd/MM/yyyy", out _))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.DateOfDeathIncorrectFormat;
            }

            if (row.LocalAuthorityCode is null || !Regex.IsMatch(row.LocalAuthorityCode, @"^\d{3}$"))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.LocalAuthorityCodeIncorrectFormat;
            }

            if (row.EstablishmentCode is not null && !Regex.IsMatch(row.EstablishmentCode, @"^\d{4}$"))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.EstablishmentNumberIncorrectFormat;
            }

            if (row.EmploymentStartDate is null || !DateOnly.TryParseExact(row.EmploymentStartDate, "dd/MM/yyyy", out _))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.EmploymentStartDateIncorrectFormat;
            }

            if (row.EmploymentEndDate is null || !DateOnly.TryParseExact(row.EmploymentEndDate, "dd/MM/yyyy", out _))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.EmploymentEndDateIncorrectFormat;
            }

            if (row.FullOrPartTimeIndicator is null || !validFullOrPartTimeIndicatorValues.Contains(row.FullOrPartTimeIndicator))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.FullOrPartTimeIndicatorIncorrectFormat;
            }

            if (row.WithdrawlIndicator is not null && row.WithdrawlIndicator != "W")
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.WithdrawlIndicatorIncorrectFormat;
            }

            if (row.ExtractDate is null || !DateOnly.TryParseExact(row.ExtractDate, "dd/MM/yyyy", out _))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.ExtractDateIncorrectFormat;
            }

            if (row.Gender is null || !validGenderValues.Contains(row.Gender))
            {
                loadErrors = loadErrors | TpsCsvExtractItemLoadErrors.GenderIncorrectFormat;
            }

            writer.StartRow();
            writer.Write(Guid.NewGuid(), NpgsqlDbType.Uuid);
            writer.Write(tpsCsvExtractId, NpgsqlDbType.Uuid);
            writer.Write(row.Trn, NpgsqlDbType.Varchar);
            writer.Write(row.NationalInsuranceNumber, NpgsqlDbType.Varchar);
            writer.Write(row.DateOfBirth, NpgsqlDbType.Varchar);
            writer.Write(row.DateOfDeath, NpgsqlDbType.Varchar);
            writer.Write(row.MemberPostcode, NpgsqlDbType.Varchar);
            writer.Write(row.MemberEmailAddress, NpgsqlDbType.Varchar);
            writer.Write(row.LocalAuthorityCode, NpgsqlDbType.Varchar);
            writer.Write(row.EstablishmentCode, NpgsqlDbType.Varchar);
            writer.Write(row.EstablishmentPostcode, NpgsqlDbType.Varchar);
            writer.Write(row.EstablishmentEmailAddress, NpgsqlDbType.Varchar);
            writer.Write(row.EmploymentStartDate, NpgsqlDbType.Varchar);
            writer.Write(row.EmploymentEndDate, NpgsqlDbType.Varchar);
            writer.Write(row.FullOrPartTimeIndicator, NpgsqlDbType.Varchar);
            writer.Write(row.WithdrawlIndicator, NpgsqlDbType.Varchar);
            writer.Write(row.ExtractDate, NpgsqlDbType.Varchar);
            writer.Write(row.Gender, NpgsqlDbType.Varchar);
            writer.Write(clock.UtcNow, NpgsqlDbType.TimestampTz);
            writer.Write((int)loadErrors, NpgsqlDbType.Integer);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CopyValidFormatDataToStaging(Guid tpsCsvExtractId, CancellationToken cancellationToken)
    {
        using var readDbContext = dbContextFactory.CreateDbContext();
        using var writeDbContext = dbContextFactory.CreateDbContext();
        var connection = (NpgsqlConnection)writeDbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var writer = await connection.BeginBinaryImportAsync(
           $"""
            COPY
                tps_csv_extract_items (
                    tps_csv_extract_item_id,
                    tps_csv_extract_id,
                    tps_csv_extract_load_item_id,                    
                    trn,
                    national_insurance_number,
                    date_of_birth,
                    date_of_death,
                    member_postcode,
                    member_email_address,
                    local_authority_code,
                    establishment_number,
                    establishment_postcode,
                    establishment_email_address,
                    employment_start_date,
                    employment_end_date,
                    employment_type,
                    withdrawl_indicator,
                    extract_date,
                    gender,                    
                    created,
                    key
                )
            FROM
                STDIN (FORMAT BINARY)
            """);

        writer.Timeout = TimeSpan.FromMinutes(5);

        await foreach (var item in readDbContext.TpsCsvExtractLoadItems.Where(x => x.TpsCsvExtractId == tpsCsvExtractId && x.Errors == TpsCsvExtractItemLoadErrors.None).AsNoTracking().AsAsyncEnumerable())
        {
            var employmentStartDate = DateOnly.ParseExact(item.EmploymentStartDate!, "dd/MM/yyyy");

            writer.StartRow();
            writer.Write(Guid.NewGuid(), NpgsqlDbType.Uuid);
            writer.Write(tpsCsvExtractId, NpgsqlDbType.Uuid);
            writer.Write(item.TpsCsvExtractLoadItemId, NpgsqlDbType.Uuid);
            writer.Write(item.Trn, NpgsqlDbType.Char);
            writer.Write(item.NationalInsuranceNumber, NpgsqlDbType.Char);
            writer.Write(DateOnly.ParseExact(item.DateOfBirth!, "dd/MM/yyyy"), NpgsqlDbType.Date);
            writer.Write(!string.IsNullOrEmpty(item.DateOfDeath) ? DateOnly.ParseExact(item.DateOfDeath, "dd/MM/yyyy") : (DateOnly?)null, NpgsqlDbType.Date);
            writer.Write(item.MemberPostcode, NpgsqlDbType.Varchar);
            writer.Write(item.MemberEmailAddress, NpgsqlDbType.Varchar);
            writer.Write(item.LocalAuthorityCode, NpgsqlDbType.Char);
            writer.Write(item.EstablishmentNumber, NpgsqlDbType.Char);
            writer.Write(item.EstablishmentPostcode, NpgsqlDbType.Varchar);
            writer.Write(item.EstablishmentEmailAddress, NpgsqlDbType.Varchar);
            writer.Write(employmentStartDate, NpgsqlDbType.Date);
            writer.Write(!string.IsNullOrEmpty(item.EmploymentEndDate) ? DateOnly.ParseExact(item.EmploymentEndDate!, "dd/MM/yyyy") : (DateOnly?)null, NpgsqlDbType.Date);
            writer.Write((int)EmploymentTypeHelper.FromFullOrPartTimeIndicator(item.FullOrPartTimeIndicator!), NpgsqlDbType.Integer);
            writer.Write(item.WithdrawlIndicator, NpgsqlDbType.Char);
            writer.Write(DateOnly.ParseExact(item.ExtractDate!, "dd/MM/yyyy"), NpgsqlDbType.Date);
            writer.Write(item.Gender, NpgsqlDbType.Varchar);
            writer.Write(clock.UtcNow, NpgsqlDbType.TimestampTz);
            writer.Write($"{item.Trn}.{item.LocalAuthorityCode}.{item.EstablishmentNumber ?? "NULL"}.{employmentStartDate:yyyyMMdd}", NpgsqlDbType.Varchar);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);
    }
}
