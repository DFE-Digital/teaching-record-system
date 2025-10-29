using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using Parquet.Serialization;
using TeachingRecordSystem.Core.Services.WorkforceData;
using TeachingRecordSystem.Core.Services.WorkforceData.Google;

namespace TeachingRecordSystem.Core.Tests.Services.WorkforceData;

[Collection(nameof(WorkforceDataTestCollection))]
public class WorkforceDataExporterTests : IAsyncLifetime
{
    public WorkforceDataExporterTests(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator)
    {
        DbFixture = dbFixture;
        Clock = new();

        TestData = new TestData(
            dbFixture.DbContextFactory,
            referenceDataCache,
            Clock,
            trnGenerator);
    }

    [Fact]
    public async Task Export_WhenCalled_ExportsDataToParquetFileAndUploadsToGcs()
    {
        // Arrange
        var optionsAccessor = Mock.Of<IOptions<WorkforceDataExportOptions>>();
        var storageClientProvider = Mock.Of<IStorageClientProvider>();
        var storageClient = Mock.Of<StorageClient>();
        var person = await TestData.CreatePersonAsync();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var personEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);

        Mock.Get(optionsAccessor)
            .Setup(o => o.Value)
            .Returns(new WorkforceDataExportOptions
            {
                BucketName = "bucket-name",
                StorageClient = storageClient
            });
        Mock.Get(storageClientProvider)
            .Setup(s => s.GetStorageClientAsync())
            .ReturnsAsync(storageClient);

        ParquetSerializer.UntypedResult? deserializedExport = null;
        Mock.Get(storageClient)
            .Setup(s => s.UploadObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<UploadObjectOptions>(), It.IsAny<CancellationToken>(), It.IsAny<IProgress<IUploadProgress>>()))
            .Callback<string, string, string, Stream, UploadObjectOptions, CancellationToken, IProgress<IUploadProgress>>(
            async (bucketName, objectName, contentType, stream, options, cancellationToken, progress) =>
            {
                deserializedExport = await ParquetSerializer.DeserializeAsync(stream, options: null, CancellationToken.None);
            })
            .ReturnsAsync(new Google.Apis.Storage.v1.Data.Object());

        // Act
        var workforceDataExporter = new WorkforceDataExporter(
            TestData.Clock,
            TestData.DbContextFactory,
            optionsAccessor,
            storageClientProvider);

        await workforceDataExporter.ExportAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(deserializedExport);
        Assert.Single(deserializedExport.Data);
        var exportItem = deserializedExport.Data[0];
        Assert.Equal(personEmployment.TpsEmploymentId, exportItem[nameof(WorkforceDataExportItem.TpsEmploymentId)]);
        Assert.Equal(personEmployment.PersonId, exportItem[nameof(WorkforceDataExportItem.PersonId)]);
        Assert.Equal(person.Trn, exportItem[nameof(WorkforceDataExportItem.Trn)]);
        Assert.Equal(establishment1.EstablishmentId, exportItem[nameof(WorkforceDataExportItem.EstablishmentId)]);
        Assert.Equal("GIAS", exportItem[nameof(WorkforceDataExportItem.EstablishmentSource)]);
        Assert.Equal(establishment1.Urn, exportItem[nameof(WorkforceDataExportItem.EstablishmentUrn)]);
        Assert.Equal(establishment1.EstablishmentName, exportItem[nameof(WorkforceDataExportItem.EstablishmentName)]);
        Assert.Equal(personEmployment.StartDate, DateOnly.FromDateTime((DateTime)exportItem[nameof(WorkforceDataExportItem.StartDate)]));
        Assert.Equal(personEmployment.LastKnownTpsEmployedDate, DateOnly.FromDateTime((DateTime)exportItem[nameof(WorkforceDataExportItem.LastKnownTpsEmployedDate)]));
        Assert.Equal("FT", exportItem[nameof(WorkforceDataExportItem.EmploymentType)]);
        Assert.False((bool)exportItem[nameof(WorkforceDataExportItem.WithdrawalConfirmed)]);
        Assert.Equal(personEmployment.LastExtractDate, DateOnly.FromDateTime((DateTime)exportItem[nameof(WorkforceDataExportItem.LastExtractDate)]));
        Assert.Equal(personEmployment.Key, exportItem[nameof(WorkforceDataExportItem.Key)]);
        Assert.Equal(nationalInsuranceNumber, exportItem[nameof(WorkforceDataExportItem.NationalInsuranceNumber)]);
        Assert.Equal(personPostcode, exportItem[nameof(WorkforceDataExportItem.PersonPostcode)]);
        Assert.Equal(personEmployment.CreatedOn, (DateTime)exportItem[nameof(WorkforceDataExportItem.CreatedOn)]);
        Assert.Equal(personEmployment.UpdatedOn, (DateTime)exportItem[nameof(WorkforceDataExportItem.UpdatedOn)]);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => DbFixture.DbHelper.ClearDataAsync();

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }
}
