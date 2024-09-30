using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.DqtReporting;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillDqtReportingAlertTypes(IOptions<DqtReportingOptions> dqtReportingOptionsAccessor, TrsDbContext dbContext, IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var conn = new SqlConnection(dqtReportingOptionsAccessor.Value.ReportingDbConnectionString);
        conn.Open();

        await BackFillAlertCategories();
        await BackFillAlertTypes();

        async Task BackFillAlertCategories()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("alert_category_id", typeof(Guid));
            dataTable.Columns.Add("name", typeof(string));
            dataTable.Columns.Add("display_order", typeof(int));
            dataTable.Columns.Add("__Inserted", typeof(DateTime));
            dataTable.Columns.Add("__Updated", typeof(DateTime));

            var alertCategories = await dbContext.AlertCategories.ToListAsync();

            foreach (var ac in alertCategories)
            {
                dataTable.Rows.Add(
                    ac.AlertCategoryId,
                    ac.Name,
                    ac.DisplayOrder,
                    clock.UtcNow,
                    clock.UtcNow);
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

        async Task BackFillAlertTypes()
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

            var alertTypes = await dbContext.AlertTypes.ToListAsync();

            foreach (var at in alertTypes)
            {
                dataTable.Rows.Add(
                    at.AlertTypeId,
                    at.AlertCategoryId,
                    at.Name,
                    at.DqtSanctionCode,
                    at.ProhibitionLevel,
                    at.IsActive,
                    at.DisplayOrder,
                    clock.UtcNow,
                    clock.UtcNow);
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
