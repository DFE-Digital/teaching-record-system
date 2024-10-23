using System.Globalization;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using CsvHelper;
using TeachingRecordSystem.Core.Dqt;

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
                    await foreach(var record in csv.GetRecordsAsync<ImportFileData>())
                    {

                    }
                }
            }
        }
        //Download Files From Storage Container
        
    }

    public Task ImportInduction()
    {
        return Task.CompletedTask;
    }

    public Task Import()
    {
        return Task.CompletedTask;
    }

    //ValidateInduction
    //Validate
}

public class ImportFileData
{
    public string FileName { get; set; }
    // Common columns
    public string Forename { get; set; }
    public string Surname { get; set; }
    public string DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string QtsRefNo { get; set; }
    public string QtsStatus { get; set; }
    public string QtsDate { get; set; }

    // ITT entity columns
    public string IttStartMonth { get; set; }
    public string IttStartYear { get; set; }
    public string IttEndDate { get; set; }
    public string IttCourseLength { get; set; }
    public string IttEstabLeaCode { get; set; }
    public string IttEstabCode { get; set; }
    public string IttQualCode { get; set; }
    public string IttClassCode { get; set; }
    public string IttSubjectCode1 { get; set; }
    public string IttSubjectCode2 { get; set; }
    public string IttMinAgeRange { get; set; }
    public string IttMaxAgeRange { get; set; }
    public string IttMinSpAgeRange { get; set; }
    public string IttMaxSpAgeRange { get; set; }
    // Qualification entity columns
    public string PqCourseLength { get; set; }
    public string PqYearOfAward { get; set; }
    public string PqCountry { get; set; }
    public string PqEstabCode { get; set; }
    public string PqQualCode { get; set; }
    public string PqHonours { get; set; }
    public string PqClassCode { get; set; }
    public string PqSubjectCode1 { get; set; }
    public string PqSubjectCode2 { get; set; }
    public string PqSubjectCode3 { get; set; }
    // Induction entity columns
    public string InductionStartDate { get; set; }
    public string InductionPassedDate { get; set; }
    public string InductionLeaName { get; set; }
    public string InductionBodyCode { get; set; }
}


