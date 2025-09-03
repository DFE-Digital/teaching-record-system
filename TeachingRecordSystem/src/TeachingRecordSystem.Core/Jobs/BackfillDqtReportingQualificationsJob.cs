using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillDqtReportingQualificationsJob(IOptions<DqtReportingOptions> dqtReportingOptionsAccessor, TrsDbContext dbContext, IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("qualification_id", typeof(Guid));
        dataTable.Columns.Add("created_on", typeof(DateTime));
        dataTable.Columns.Add("updated_on", typeof(DateTime));
        dataTable.Columns.Add("deleted_on", typeof(DateTime));
        dataTable.Columns.Add("qualification_type", typeof(int));
        dataTable.Columns.Add("person_id", typeof(Guid));
        dataTable.Columns.Add("dqt_qualification_id", typeof(Guid));
        dataTable.Columns.Add("mq_specialism", typeof(int));
        dataTable.Columns.Add("mq_status", typeof(int));
        dataTable.Columns.Add("start_date", typeof(DateOnly));
        dataTable.Columns.Add("end_date", typeof(DateOnly));
        dataTable.Columns.Add("dqt_mq_establishment_id", typeof(Guid));
        dataTable.Columns.Add("dqt_specialism_id", typeof(Guid));
        dataTable.Columns.Add("mq_provider_id", typeof(Guid));
        dataTable.Columns.Add("__Inserted", typeof(DateTime));
        dataTable.Columns.Add("__Updated", typeof(DateTime));

        var mqs = await dbContext.MandatoryQualifications.ToListAsync();

        foreach (var mq in mqs)
        {
            dataTable.Rows.Add(
                mq.QualificationId,
                mq.CreatedOn,
                mq.UpdatedOn,
                mq.DeletedOn,
                mq.QualificationType,
                mq.PersonId,
                mq.DqtQualificationId,
                mq.Specialism,
                mq.Status,
                mq.StartDate,
                mq.EndDate,
                mq.DqtMqEstablishmentId,
                mq.DqtSpecialismId,
                mq.ProviderId,
                clock.UtcNow,
                clock.UtcNow);
        }

        using var conn = new SqlConnection(dqtReportingOptionsAccessor.Value.ReportingDbConnectionString);
        conn.Open();

        using (var sqlBulkCopy = new SqlBulkCopy(conn))
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
            }

            sqlBulkCopy.BulkCopyTimeout = 0;
            sqlBulkCopy.DestinationTableName = "trs_qualifications";

            await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
        }
    }
}
