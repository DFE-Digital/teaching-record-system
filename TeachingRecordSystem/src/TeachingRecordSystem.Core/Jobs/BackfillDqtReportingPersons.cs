using System.Data;
using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class BackfillDqtReportingPersons(IOptions<DqtReportingOptions> dqtReportingOptionsAccessor, TrsDbContext dbContext, IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        using var conn = new SqlConnection(dqtReportingOptionsAccessor.Value.ReportingDbConnectionString);
        conn.Open();

        using (var truncateCmd = conn.CreateCommand())
        {
            truncateCmd.CommandTimeout = 0;
            truncateCmd.CommandText = "truncate table trs_persons";
            await truncateCmd.ExecuteNonQueryAsync();
        }

        var txn = conn.BeginTransaction();

        var dataTable = new DataTable();
        dataTable.Columns.Add("person_id", typeof(Guid));
        dataTable.Columns.Add("trn", typeof(string));
        dataTable.Columns.Add("first_name", typeof(string));
        dataTable.Columns.Add("middle_name", typeof(string));
        dataTable.Columns.Add("last_name", typeof(string));
        dataTable.Columns.Add("date_of_birth", typeof(DateOnly));
        dataTable.Columns.Add("email_address", typeof(string));
        dataTable.Columns.Add("national_insurance_number", typeof(string));
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

        await foreach (var chunk in dbContext.Persons.AsNoTracking().AsAsyncEnumerable().Chunk(200))
        {
            var personList = chunk.ToList();

            while (personList.Count > 0)
            {
                foreach (var e in personList)
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

                try
                {
                    await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                    continue;
                }
                catch (SqlException ex) when (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    var key = Guid.Parse(ex.Message.Substring(
                        ex.Message.IndexOf("The duplicate key value is (") + "The duplicate key value is (".Length,
                        Guid.Empty.ToString().Length));

                    personList.RemoveAll(p => p.PersonId == key);
                    dataTable.Rows.Clear();
                }
            }

            dataTable.Rows.Clear();
        }

        await txn.CommitAsync();
    }
}
