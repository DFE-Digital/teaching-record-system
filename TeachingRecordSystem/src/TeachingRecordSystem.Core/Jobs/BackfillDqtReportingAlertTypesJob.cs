using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillDqtReportingAlertTypesJob(IOptions<DqtReportingOptions> dqtReportingOptionsAccessor, TrsDbContext dbContext, TimeProvider timeProvider)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(dqtReportingOptionsAccessor.Value.ReportingDbConnectionString);
        conn.Open();

        await BackFillAlertCategoriesAsync();
        await BackFillAlertTypesAsync();

        async Task BackFillAlertCategoriesAsync()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("alert_category_id", typeof(Guid));
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Columns.Add("display_order", typeof(int));
            dataTable.Columns.Add("__Inserted", typeof(DateTime));
            dataTable.Columns.Add("__Updated", typeof(DateTime));

            var alertCategories = await dbContext.AlertCategories.ToListAsync(cancellationToken: cancellationToken);

            foreach (var ac in alertCategories)
            {
                dataTable.Rows.Add(
                    ac.AlertCategoryId,
                    ac.Name,
                    ac.DisplayOrder,
                    timeProvider.UtcNow,
                    timeProvider.UtcNow);
            }

            using (var sqlBulkCopy = new SqlBulkCopy(conn))
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                }

                sqlBulkCopy.BulkCopyTimeout = 0;
                sqlBulkCopy.DestinationTableName = "trs_alert_categories";

                await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
            }
        }

        async Task BackFillAlertTypesAsync()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("alert_type_id", typeof(Guid));
            dataTable.Columns.Add("alert_category_id", typeof(Guid));
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Columns.Add("dqt_sanction_code", typeof(string));
            dataTable.Columns.Add("internal_only", typeof(bool));
            dataTable.Columns.Add("is_active", typeof(bool));
            dataTable.Columns.Add("display_order", typeof(int));
            dataTable.Columns.Add("__Inserted", typeof(DateTime));
            dataTable.Columns.Add("__Updated", typeof(DateTime));

            var alertTypes = await dbContext.AlertTypes.ToListAsync(cancellationToken: cancellationToken);

            foreach (var at in alertTypes)
            {
                dataTable.Rows.Add(
                    at.AlertTypeId,
                    at.AlertCategoryId,
                    at.Name,
                    at.DqtSanctionCode,
                    at.InternalOnly,
                    at.IsActive,
                    at.DisplayOrder,
                    timeProvider.UtcNow,
                    timeProvider.UtcNow);
            }

            using (var sqlBulkCopy = new SqlBulkCopy(conn))
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                }

                sqlBulkCopy.BulkCopyTimeout = 0;
                sqlBulkCopy.DestinationTableName = "trs_alert_types";

                await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
            }
        }
    }
}
