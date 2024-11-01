using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.EWCWalesImport;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class EWCWalesImportJobTests : IAsyncLifetime
{
    public EWCWalesImportJobTests(
      DbFixture dbFixture,
      IOrganizationServiceAsync2 organizationService,
      ReferenceDataCache referenceDataCache,
      FakeTrnGenerator trnGenerator,
      IServiceProvider provider)
    {
        DbFixture = dbFixture;
        Clock = new();
        Helper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock);

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));
        var blobServiceClient = new Mock<BlobServiceClient>();

        var importer = ActivatorUtilities.CreateInstance<QTSImporter>(provider);
        var importer2 = ActivatorUtilities.CreateInstance<InductionImporter>(provider);
        Job = ActivatorUtilities.CreateInstance<EWCWalesImportJob>(provider, blobServiceClient.Object, importer, importer2);
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }

    Task IAsyncLifetime.InitializeAsync() => DbFixture.WithDbContext(dbContext => dbContext.Events.ExecuteDeleteAsync());

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public EWCWalesImportJob Job { get; }
    public ICrmQueryDispatcher _QueryDispatcher { get; } 


    [Theory]
    [InlineData("IND", EWCWalesImportFileType.Induction)]
    [InlineData("QTS", EWCWalesImportFileType.Qualification)]
    public void EWCWalesImportJob_GetImportFileType_ReturnsExpected(string filename, EWCWalesImportFileType importType)
    {
        var type = Job.GetImporFileType(filename);
        Assert.Equal(type, importType);
    }

    [Fact]
    public async Task EWCWalesImportJob_ImportsQtsFileSuccessfully()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname, 01/01/1987,49, 04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        using (var reader = Job.GetStreamReader(csvBytes))
        {
            await Job.Import("QTS", reader);
        }

        // Assert
        //Asserts success count
        //Asserts FailureCount

    }

    [Fact]
    public async Task EWCWalesImportJob_ImportsInductionFileSuccessfully()
    {
        // Arrange
        var account = await TestData.CreateAccount(x => x.WithName("SomeName"));
        var person = await TestData.CreatePerson();
        var trn = person.Trn;
        var csvContent = $"REFERENCE_NO,FIRST_NAME,LAST_NAME,DATE_OF_BIRTH,START_DATE,PASS_DATE,FAIL_DATE,EMPLOYER_NAME,EMPLOYER_CODE,IND_STATUS_NAME\r\n{trn},{person.FirstName},{person.LastName},{person.DateOfBirth},17/09/2014,28/09/2017,,{account.Name},668,Pass\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);

        // Act
        using (var reader = Job.GetStreamReader(csvBytes))
        {
            await Job.Import("IND", reader);
        }

        //Assert

    }
}
