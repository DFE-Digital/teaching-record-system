using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillDqtReportingWorkforceDataJob(IOptions<DqtReportingOptions> dqtReportingOptionsAccessor, TrsDbContext dbContext, TimeProvider timeProvider)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SyncEmploymentsAsync();
        await SyncTpsEstablishmentsAsync();
        await SyncTpsEmploymentsAsync();

        async Task SyncEmploymentsAsync()
        {
            dbContext.Database.SetCommandTimeout(0);

            using var conn = new SqlConnection(dqtReportingOptionsAccessor.Value.ReportingDbConnectionString);
            conn.Open();
            var txn = conn.BeginTransaction();

            var dataTable = new DataTable();
            dataTable.Columns.Add("establishment_id", typeof(Guid));
            dataTable.Columns.Add("urn", typeof(int));
            dataTable.Columns.Add("la_code", typeof(string));
            dataTable.Columns.Add("la_name", typeof(string));
            dataTable.Columns.Add("establishment_number", typeof(string));
            dataTable.Columns.Add("establishment_name", typeof(string));
            dataTable.Columns.Add("establishment_type_code", typeof(string));
            dataTable.Columns.Add("establishment_type_name", typeof(string));
            dataTable.Columns.Add("establishment_type_group_code", typeof(int));
            dataTable.Columns.Add("establishment_type_group_name", typeof(string));
            dataTable.Columns.Add("establishment_status_code", typeof(int));
            dataTable.Columns.Add("establishment_status_name", typeof(string));
            dataTable.Columns.Add("street", typeof(string));
            dataTable.Columns.Add("locality", typeof(string));
            dataTable.Columns.Add("address3", typeof(string));
            dataTable.Columns.Add("town", typeof(string));
            dataTable.Columns.Add("county", typeof(string));
            dataTable.Columns.Add("postcode", typeof(string));
            dataTable.Columns.Add("establishment_source_id", typeof(int));
            dataTable.Columns.Add("__Inserted", typeof(DateTime));
            dataTable.Columns.Add("__Updated", typeof(DateTime));

            using var sqlBulkCopy = new SqlBulkCopy(conn, new SqlBulkCopyOptions(), txn);
            sqlBulkCopy.BulkCopyTimeout = 0;
            sqlBulkCopy.DestinationTableName = "trs_establishments";

            foreach (DataColumn column in dataTable.Columns)
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
            }

            await foreach (var chunk in dbContext.Establishments.AsNoTracking().AsAsyncEnumerable().ChunkAsync(200))
            {
                foreach (var e in chunk)
                {
                    dataTable.Rows.Add(
                        e.EstablishmentId,
                        e.Urn,
                        e.LaCode,
                        e.LaName,
                        e.EstablishmentNumber,
                        e.EstablishmentName,
                        e.EstablishmentTypeCode,
                        e.EstablishmentTypeName,
                        e.EstablishmentTypeGroupCode,
                        e.EstablishmentTypeGroupName,
                        e.EstablishmentStatusCode,
                        e.EstablishmentStatusName,
                        e.Street,
                        e.Locality,
                        e.Address3,
                        e.Town,
                        e.County,
                        e.Postcode,
                        e.EstablishmentSourceId,
                        timeProvider.UtcNow,
                        timeProvider.UtcNow);
                }

                await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);

                dataTable.Rows.Clear();
            }

            await txn.CommitAsync(cancellationToken);
        }

        async Task SyncTpsEstablishmentsAsync()
        {
            dbContext.Database.SetCommandTimeout(0);

            using var conn = new SqlConnection(dqtReportingOptionsAccessor.Value.ReportingDbConnectionString);
            conn.Open();
            var txn = conn.BeginTransaction();

            var dataTable = new DataTable();
            dataTable.Columns.Add("tps_establishment_id", typeof(Guid));
            dataTable.Columns.Add("la_code", typeof(string));
            dataTable.Columns.Add("establishment_code", typeof(string));
            dataTable.Columns.Add("employers_name", typeof(string));
            dataTable.Columns.Add("school_gias_name", typeof(string));
            dataTable.Columns.Add("school_closed_date", typeof(DateOnly));
            dataTable.Columns.Add("__Inserted", typeof(DateTime));
            dataTable.Columns.Add("__Updated", typeof(DateTime));

            using var sqlBulkCopy = new SqlBulkCopy(conn, new SqlBulkCopyOptions(), txn);
            sqlBulkCopy.BulkCopyTimeout = 0;
            sqlBulkCopy.DestinationTableName = "trs_tps_establishments";

            foreach (DataColumn column in dataTable.Columns)
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
            }

            await foreach (var chunk in dbContext.TpsEstablishments.AsNoTracking().AsAsyncEnumerable().ChunkAsync(200))
            {
                foreach (var e in chunk)
                {
                    dataTable.Rows.Add(
                        e.TpsEstablishmentId,
                        e.LaCode,
                        e.EstablishmentCode,
                        e.EmployersName,
                        e.SchoolGiasName,
                        e.SchoolClosedDate,
                        timeProvider.UtcNow,
                        timeProvider.UtcNow);
                }

                await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);

                dataTable.Rows.Clear();
            }

            await txn.CommitAsync(cancellationToken);
        }

        async Task SyncTpsEmploymentsAsync()
        {
            dbContext.Database.SetCommandTimeout(0);

            using var conn = new SqlConnection(dqtReportingOptionsAccessor.Value.ReportingDbConnectionString);
            await conn.OpenAsync(cancellationToken);
            var txn = conn.BeginTransaction();

            var dataTable = new DataTable();
            dataTable.Columns.Add("tps_employment_id", typeof(Guid));
            dataTable.Columns.Add("person_id", typeof(Guid));
            dataTable.Columns.Add("establishment_id", typeof(Guid));
            dataTable.Columns.Add("start_date", typeof(DateOnly));
            dataTable.Columns.Add("end_date", typeof(DateOnly));
            dataTable.Columns.Add("last_known_tps_employed_date", typeof(DateOnly));
            dataTable.Columns.Add("last_extract_date", typeof(DateOnly));
            dataTable.Columns.Add("employment_type", typeof(int));
            dataTable.Columns.Add("created_on", typeof(DateTime));
            dataTable.Columns.Add("updated_on", typeof(DateTime));
            dataTable.Columns.Add("key", typeof(string));
            dataTable.Columns.Add("national_insurance_number", typeof(string));
            dataTable.Columns.Add("person_postcode", typeof(string));
            dataTable.Columns.Add("withdrawal_confirmed", typeof(bool));
            dataTable.Columns.Add("__Inserted", typeof(DateTime));
            dataTable.Columns.Add("__Updated", typeof(DateTime));

            using var sqlBulkCopy = new SqlBulkCopy(conn, new SqlBulkCopyOptions(), txn);
            sqlBulkCopy.BulkCopyTimeout = 0;
            sqlBulkCopy.DestinationTableName = "trs_tps_employments";

            foreach (DataColumn column in dataTable.Columns)
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
            }

            await foreach (var chunk in dbContext.TpsEmployments.AsNoTracking().AsAsyncEnumerable().ChunkAsync(200))
            {
                foreach (var e in chunk)
                {
                    dataTable.Rows.Add(
                        e.TpsEmploymentId,
                        e.PersonId,
                        e.EstablishmentId,
                        e.StartDate,
                        e.EndDate,
                        e.LastKnownTpsEmployedDate,
                        e.LastExtractDate,
                        e.EmploymentType,
                        e.CreatedOn,
                        e.UpdatedOn,
                        e.Key,
                        e.NationalInsuranceNumber,
                        e.PersonPostcode,
                        e.WithdrawalConfirmed,
                        timeProvider.UtcNow,
                        timeProvider.UtcNow);
                }

                await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);

                dataTable.Rows.Clear();
            }

            await txn.CommitAsync(cancellationToken);
        }
    }
}
