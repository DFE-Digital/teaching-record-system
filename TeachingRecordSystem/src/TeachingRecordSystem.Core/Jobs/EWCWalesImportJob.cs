using System.Globalization;
using System.Text.RegularExpressions;
using Azure;
using Azure.Storage.Blobs;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs;

public class EWCWalesImportJob(ICrmQueryDispatcher crmQueryDispatcher, BlobServiceClient blobServiceClient)
{
    private readonly Regex _dateRegex = new Regex(@"^\d{8}$", RegexOptions.Compiled);
    private readonly Regex _trnRegex = new Regex(@"^\d{7}$", RegexOptions.Compiled);
    private readonly Regex _numberCheckRegex = new Regex(@"(\d)+", RegexOptions.Compiled);
    private readonly string _storageContainer = "ewc-wales-import";
    private readonly string _processedFolder = "processed";
    private readonly string _pickupFolder = "pickup";

    public async Task Execute(CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_storageContainer);
        await foreach (var blob in containerClient.GetBlobsAsync())
        {
            BlobClient blobClient = containerClient.GetBlobClient(blob.Name);
            using (var downloadStream = new MemoryStream())
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

                    try
                    {
                        var importType = blob.Name.ToLower() switch
                        {
                            var filename when filename.Contains("Ind", StringComparison.OrdinalIgnoreCase) => EWCWalesImportFileType.Induction,
                            var filename when filename.Contains("QTS", StringComparison.OrdinalIgnoreCase) => EWCWalesImportFileType.Qualification,
                            _ => throw new InvalidOperationException("Not matching EWCWales Filename")
                        };


                        if (importType == EWCWalesImportFileType.Induction)
                        {
                            var records = csv.GetRecords<EWCWalesInductionFileImportData>().ToList();

                            var s = "";
                        }
                        else if (importType == EWCWalesImportFileType.Qualification)
                        {
                            var records = csv.GetRecords<EWCWalesQTSFileImportData>().ToList();

                            var s = "";
                        }


                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.InnerException);
                    }

                    var updateQuery = new UpdateIntegrationTransactionQuery()
                    {
                        IntegrationTransactionId = integrationId,
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


public class EWCWalesInductionFileImportData
{
    [Name("REFERENCE_NO")]
    public string ReferenceNumber { get; init; }

    [Name("FIRST_NAME")]
    public string FirstName { get; init; }

    [Name("LAST_NAME")]
    public string LastName { get; init; }

    [Name("DATE_OF_BIRTH")]
    public string DateOfBirth { get; init; }

    [Name("START_DATE")]
    public string StartDate { get; init; }

    [Name("PASS_DATE")]
    public string PassDate { get; init; }

    [Name("FAIL_DATE")]
    public string FailDate { get; init; }

    [Name("EMPLOYER_NAME")]
    public string EmployerName { get; init; }

    [Name("EMPLOYER_CODE")]
    public string EmployerCode { get; init; }

    [Name("IND_STATUS_NAME")]
    public string InductionStatusName { get; init; }
}

public class EWCWalesQTSFileImportData
{
    [Name("QTS_REF_NO")]
    public string QtsRefNo { get; init; }

    [Name("FORENAME")]
    public string Forename { get; init; }

    [Name("SURNAME")]
    public string Surname { get; init; }

    [Name("DATE_OF_BIRTH")]
    public string DateOfBirth { get; init; }

    [Name("QTS_STATUS")]
    public string QtsStatus { get; init; }

    [Name("QTS_DATE")]
    public string QtsDate { get; set; }

    [Name("ITT StartMONTH")]
    public string IttStartMonth { get; init; }

    [Name("ITT START YY")]
    public string IttStartYear { get; init; }

    [Name("ITT End Date")]
    public string IttEndDate { get; init; }



    [Name("ITT Course Length")]
    public string ITTCourseLength { get; init; }

    [Name("ITT Estab LEA code")]
    public string IttEstabLeaCode { get; init; }

    [Name("ITT Estab Code")]
    public string IttEstabCode { get; init; }

    [Name("ITT Qual Code")]
    public string IttQualCode { get; init; }

    [Name("ITT Class Code")]
    public string IttClassCode { get; init; }

    [Name("ITT Subject Code 1")]
    public string IttSubjectCode1 { get; init; }

    [Name("ITT Subject Code 2")]
    public string IttSubjectCode2 { get; init; }

    [Name("ITT Min Age Range")]
    public string IttMinAgeRange { get; init; }

    [Name("ITT Max Age Range")]
    public string IttMaxAgeRange { get; init; }

    [Name("ITT Min Sp Age Range")]
    public string IttMinSpAgeRange { get; init; }

    [Name("ITT Max Sp Age Range")]
    public string IttMaxSpAgeRange { get; init; }

    [Name("ITT StartMONTH")]
    public string IttCourseLength { get; init; }

    [Name("PQ Year of Award")]
    public string PqYearOfAward { get; init; }

    [Name("COUNTRY")]
    public string Country { get; init; }

    [Name("PQ Estab Code")]
    public string PqEstabCode { get; init; }

    [Name("PQ Qual Code")]
    public string PqQualCode { get; init; }

    [Name("HONOURS")]
    public string Honours { get; init; }

    [Name("PQ Class Code")]
    public string PqClassCode { get; init; }

    [Name("PQ Subject Code 1")]
    public string PqSubjectCode1 { get; init; }

    [Name("PQ Subject Code 2")]
    public string PqSubjectCode2 { get; init; }

    [Name("PQ Subject Code 3")]
    public string PqSubjectCode3 { get; init; }








    //public string Gender { get; init; }

    //// ITT entity columns
    //// Qualification entity columns
    //public string PqCourseLength { get; init; }
    //// Induction entity columns
    //public string InductionStartDate { get; init; }
    //public string InductionPassedDate { get; init; }
    //public string InductionLeaName { get; init; }
    //public string InductionBodyCode { get; init; }
}


