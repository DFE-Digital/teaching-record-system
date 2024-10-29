using System.Globalization;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using CsvHelper;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs;

public class EWCWalesImportJob(ICrmQueryDispatcher crmQueryDispatcher, ReferenceDataCache referenceDataCache, IClock clock)
{
    private readonly Regex _dateRegex = new Regex(@"^\d{8}$", RegexOptions.Compiled);
    private readonly Regex _trnRegex = new Regex(@"^\d{7}$", RegexOptions.Compiled);
    private readonly Regex _numberCheckRegex = new Regex(@"(\d)+", RegexOptions.Compiled);

    public async Task Execute(CancellationToken cancellationToken)
    {
        BlobServiceClient  blobServiceClient = new BlobServiceClient("");
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("EWCWalesImport");
        await foreach(var blob in containerClient.GetBlobsAsync())
        {
            BlobClient blobClient = containerClient.GetBlobClient(blob.Name);
            using(var downloadStream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(downloadStream);
                downloadStream.Position = 0;


                using (var reader = new StreamReader(downloadStream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var integrationJob = new CreateIntegrationTransactionQuery()
                    {
                        StartDate = DateTime.Now,
                        TypeId = (int)dfeta_IntegrationInterface.GTCWalesImport
                    };
                    var integrationId = await crmQueryDispatcher.ExecuteQuery(integrationJob);

                    await foreach(var record in csv.GetRecordsAsync<ImportFileData>())
                    {
                        var importType = record.FileName.ToLower() switch
                        {
                            var filename when filename.Contains("Ind", StringComparison.OrdinalIgnoreCase) => EWCWalesImportFileType.Induction,
                            _ => EWCWalesImportFileType.Qualification
                        };

                        if(importType == EWCWalesImportFileType.Induction)
                        {
                            await ImportInduction();
                        }
                        else
                        {
                            await ImportQualification();
                        }
                    }

                    var updateQuery = new UpdateIntegrationTransactionQuery()
                    {
                        EndDate = DateTime.Now,
                        TotalCount = 0,
                        SuccessCount = 0,
                        FailureCount = 0,
                        DuplicateCount = 0,
                        FailureMessage = ""
                    };
                    await crmQueryDispatcher.ExecuteQuery(updateQuery);

                }
            }
        }
    }

    public Task ImportInduction()
    {
        return Task.CompletedTask;
    }

    public Task ImportQualification()
    {
        return Task.CompletedTask;
    }
}


public enum EWCWalesImportFileType
{
    Induction = 1,
    Qualification = 2
}


public class ImportFileData
{
    public string FileName { get; init; }
    // Common columns
    public string Forename { get; init; }
    public string Surname { get; init; }
    public string DateOfBirth { get; init; }
    public string Gender { get; init; }
    public string QtsRefNo { get; init; }
    public string QtsStatus { get; init; }
    public string QtsDate { get; set; }

    // ITT entity columns
    public string IttStartMonth { get; init; }
    public string IttStartYear { get; init; }
    public string IttEndDate { get; init; }
    public string IttCourseLength { get; init; }
    public string IttEstabLeaCode { get; init; }
    public string IttEstabCode { get; init; }
    public string IttQualCode { get; init; }
    public string IttClassCode { get; init; }
    public string IttSubjectCode1 { get; init; }
    public string IttSubjectCode2 { get; init; }
    public string IttMinAgeRange { get; init; }
    public string IttMaxAgeRange { get; init; }
    public string IttMinSpAgeRange { get; init; }
    public string IttMaxSpAgeRange { get; init; }
    // Qualification entity columns
    public string PqCourseLength { get; init; }
    public string PqYearOfAward { get; init; }
    public string PqCountry { get; init; }
    public string PqEstabCode { get; init; }
    public string PqQualCode { get; init; }
    public string PqHonours { get; init; }
    public string PqClassCode { get; init; }
    public string PqSubjectCode1 { get; init; }
    public string PqSubjectCode2 { get; init; }
    public string PqSubjectCode3 { get; init; }
    // Induction entity columns
    public string InductionStartDate { get; init; }
    public string InductionPassedDate { get; init; }
    public string InductionLeaName { get; init; }
    public string InductionBodyCode { get; init; }
}


