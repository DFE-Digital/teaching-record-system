using Microsoft.Extensions.Options;
using Parquet.Serialization;
using Parquet.Utils;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.WorkforceData.Google;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class WorkforceDataExporter(
    IClock clock,
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IOptions<WorkforceDataExportOptions> optionsAccessor,
    IStorageClientProvider storageClientProvider)
{
    public async Task ExportAsync(CancellationToken cancellationToken)
    {
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.SetCommandTimeout(300);

        FormattableString querySql =
            $"""
            SELECT
            	t.tps_employment_id,
            	t.person_id,
            	p.trn,
            	e.establishment_id,
            	s.name establishment_source,
            	e.urn establishment_urn,
            	e.la_code local_authority_code,
            	e.establishment_number,
            	e.establishment_name,
            	t.start_date,
            	t.end_date,
            	t.last_known_tps_employed_date,
            	CASE
            		WHEN t.employment_type = 0 THEN 'FT'
            		WHEN t.employment_type = 1 THEN 'PTR'
            		WHEN t.employment_type = 2 THEN 'PTI'
            		WHEN t.employment_type = 3 THEN 'PT'
            	END employment_type,
            	t.withdrawal_confirmed,
            	t.last_extract_date,
            	t.key,
            	t.national_insurance_number,
            	t.person_postcode,
            	t.created_on,
            	t.updated_on
            FROM
            		tps_employments t
            	JOIN
            		persons p ON p.person_id = t.person_id
            	JOIN
            		establishments e ON e.establishment_id = t.establishment_id
            	JOIN
            		establishment_sources s ON s.establishment_source_id = e.establishment_source_id
            """;

        var fileDateTime = clock.UtcNow.ToString("yyyyMMddHHmm");
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"workforce_data_{fileDateTime}");
        Directory.CreateDirectory(tempDirectory);

        var i = 0;
        var fileNumber = 0;
        var itemsToExport = new List<WorkforceDataExportItem>();
        await foreach (var item in dbContext.Database.SqlQuery<WorkforceDataExportItem>(querySql).AsAsyncEnumerable())
        {
            i++;
            itemsToExport.Add(item);

            if (i % 50000 == 0)
            {
                fileNumber++;
                await ParquetSerializer.SerializeAsync(itemsToExport, Path.Combine(tempDirectory, $"workforce_data_{fileDateTime}_{fileNumber}.parquet"), cancellationToken: cancellationToken);
                itemsToExport.Clear();
            }
        }

        if (itemsToExport.Count > 0)
        {
            fileNumber++;
            await ParquetSerializer.SerializeAsync(itemsToExport, Path.Combine(tempDirectory, $"workforce_data_{fileDateTime}_{fileNumber}.parquet"), cancellationToken: cancellationToken);
            itemsToExport.Clear();
        }

        using var stream = new MemoryStream();
        var merger = new FileMerger(new DirectoryInfo(tempDirectory));
        await merger.MergeFilesAsync(stream, cancellationToken: cancellationToken);
        await UploadFileAsync(stream, $"workforce_data_{clock.UtcNow:yyyyMMddHHmm}.parquet", cancellationToken);
        Directory.Delete(tempDirectory, true);
    }

    private async Task UploadFileAsync(Stream stream, string fileName, CancellationToken cancellationToken)
    {
        var storageClient = await storageClientProvider.GetStorageClientAsync();
        var options = optionsAccessor.Value;
        options.ValidateOptions();

        await storageClient.UploadObjectAsync(options.BucketName, fileName, null, stream, cancellationToken: cancellationToken);
    }
}
