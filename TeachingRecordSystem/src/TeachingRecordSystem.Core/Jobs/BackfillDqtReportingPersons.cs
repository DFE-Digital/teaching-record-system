using System.Data;
using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class BackfillDqtReportingPersons(IConfiguration configuration, TrsDbContext dbContext, IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        using var conn = new SqlConnection(configuration.GetRequiredValue("DqtReporting:ReportingDbConnectionString"));
        conn.Open();

        var txn = conn.BeginTransaction();

        using (var truncateCmd = conn.CreateCommand())
        {
            truncateCmd.Transaction = txn;
            truncateCmd.CommandText = "truncate table trs_persons";
            await truncateCmd.ExecuteNonQueryAsync();
        }

        var dataTable = new DataTable();
        dataTable.Columns.Add("person_id", typeof(Guid));
        dataTable.Columns.Add("trn", typeof(string));
        dataTable.Columns.Add("first_name", typeof(string));
        dataTable.Columns.Add("middle_name", typeof(string));
        dataTable.Columns.Add("last_name", typeof(string));
        dataTable.Columns.Add("date_of_birth", typeof(DateOnly));
        dataTable.Columns.Add("email_address", typeof(string));
        dataTable.Columns.Add("national_insurance_number", typeof(string));
        dataTable.Columns.Add("mobile_number", typeof(string));
        dataTable.Columns.Add("gender", typeof(int));
        dataTable.Columns.Add("dqt_contact_id", typeof(Guid));
        dataTable.Columns.Add("dqt_state", typeof(int));
        dataTable.Columns.Add("dqt_first_name", typeof(string));
        dataTable.Columns.Add("dqt_middle_name", typeof(string));
        dataTable.Columns.Add("dqt_last_name", typeof(string));
        dataTable.Columns.Add("dqt_first_sync", typeof(DateTime));
        dataTable.Columns.Add("dqt_last_sync", typeof(DateTime));
        dataTable.Columns.Add("dqt_created_on", typeof(DateTime));
        dataTable.Columns.Add("dqt_modified_on", typeof(DateTime));
        dataTable.Columns.Add("created_on", typeof(DateTime));
        dataTable.Columns.Add("deleted_on", typeof(DateTime));
        dataTable.Columns.Add("updated_on", typeof(DateTime));
        dataTable.Columns.Add("__Inserted", typeof(DateTime));
        dataTable.Columns.Add("__Updated", typeof(DateTime));

        using var sqlBulkCopy = new SqlBulkCopy(conn, new SqlBulkCopyOptions(), txn);
        sqlBulkCopy.BulkCopyTimeout = 0;
        sqlBulkCopy.DestinationTableName = "trs_persons";

        foreach (DataColumn column in dataTable.Columns)
        {
            sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
        }

        await foreach (var chunk in dbContext.Persons.AsNoTracking().AsAsyncEnumerable().ChunkAsync(200).WithCancellation(cancellationToken))
        {
            foreach (var e in chunk)
            {
                dataTable.Rows.Add(
                    e.PersonId,
                    e.Trn,
                    e.FirstName,
                    e.MiddleName,
                    e.LastName,
                    e.DateOfBirth,
                    e.EmailAddress,
                    e.NationalInsuranceNumber,
                    e.MobileNumber,
                    (int?)e.Gender,
                    e.DqtContactId,
                    e.DqtState,
                    e.DqtFirstName,
                    e.DqtMiddleName,
                    e.DqtLastName,
                    e.DqtFirstSync,
                    e.DqtLastSync,
                    e.DqtCreatedOn,
                    e.DqtModifiedOn,
                    e.CreatedOn,
                    e.DeletedOn,
                    e.UpdatedOn,
                    clock.UtcNow,
                    clock.UtcNow);
            }

            await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);

            dataTable.Rows.Clear();
        }

        await txn.CommitAsync();
    }
}
