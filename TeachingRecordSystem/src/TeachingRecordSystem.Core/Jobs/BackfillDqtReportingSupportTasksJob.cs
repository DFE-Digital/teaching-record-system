using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillDqtReportingSupportTasksJob(IOptions<DqtReportingOptions> dqtReportingOptionsAccessor, TrsDbContext dbContext, TimeProvider timeProvider)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("support_task_reference", typeof(string));
        dataTable.Columns.Add("support_task_type", typeof(int));
        dataTable.Columns.Add("status", typeof(int));
        dataTable.Columns.Add("data", typeof(string));
        dataTable.Columns.Add("one_login_user_subject", typeof(string));
        dataTable.Columns.Add("person_id", typeof(Guid));
        dataTable.Columns.Add("created_on", typeof(DateTime));
        dataTable.Columns.Add("updated_on", typeof(DateTime));
        dataTable.Columns.Add("trn_request_application_user_id", typeof(Guid));
        dataTable.Columns.Add("trn_request_id", typeof(string));
        dataTable.Columns.Add("__Inserted", typeof(DateTime));
        dataTable.Columns.Add("__Updated", typeof(DateTime));

        var supportTasks = await dbContext.SupportTasks.AsNoTracking().ToListAsync(cancellationToken);

        foreach (var task in supportTasks)
        {
            dataTable.Rows.Add(
                task.SupportTaskReference,
                (int)task.SupportTaskType,
                (int)task.Status,
                JsonSerializer.Serialize(task.Data, ISupportTaskData.SerializerOptions),
                task.OneLoginUserSubject,
                task.PersonId,
                task.CreatedOn,
                task.UpdatedOn,
                task.TrnRequestApplicationUserId,
                task.TrnRequestId,
                timeProvider.UtcNow,
                timeProvider.UtcNow);
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
            sqlBulkCopy.DestinationTableName = "trs_support_tasks";

            await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
        }
    }
}
