using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public partial class EwcWalesImportJobTests(EwcWalesImportJobFixture fixture) : IClassFixture<EwcWalesImportJobFixture>
{
    private IDbContextFactory<TrsDbContext> DbContextFactory => Fixture.DbContextFactory;

    private IClock Clock => Fixture.Clock;

    private TestData TestData => Fixture.TestData;

    private EwcWalesImportJobFixture Fixture { get; } = fixture;

    private async Task<T> WithJob<T>(Func<EwcWalesImportJob, Task<T>> action)
    {
        await using var scope = Fixture.Services.CreateAsyncScope();
        var job = scope.ServiceProvider.GetRequiredService<EwcWalesImportJob>();
        return await action(job);
    }

    private async Task WithJob(Func<EwcWalesImportJob, Task> action)
    {
        await using var scope = Fixture.Services.CreateAsyncScope();
        var job = scope.ServiceProvider.GetRequiredService<EwcWalesImportJob>();
        await action(job);
    }

    [Theory]
    [InlineData("IND", EwcWalesImportFileType.Induction)]
    [InlineData("QTS", EwcWalesImportFileType.Qualification)]
    [InlineData("", null)]
    public async Task EwcWalesImportJob_GetImportFileType_ReturnsExpected(string filename, EwcWalesImportFileType? importType)
    {
        await WithJob(job =>
        {
            job.TryGetImportFileType(filename, out var type);
            Assert.Equal(importType, type);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task EwcWalesImportJob_ImportsInvalidFileType_ReturnsExpected()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var trn = person.Trn;
        var csvContent = $"QTS_REF_NO,FORENAME,SURNAME,DATE_OF_BIRTH,QTS_STATUS,QTS_DATE,ITT StartMONTH,ITT START YY,ITT End Date,ITT Course Length,ITT Estab LEA code,ITT Estab Code,ITT Qual Code,ITT Class Code,ITT Subject Code 1,ITT Subject Code 2,ITT Min Age Range,ITT Max Age Range,ITT Min Sp Age Range,ITT Max Sp Age Range,ITT Course Length,PQ Year of Award,COUNTRY,PQ Estab Code,PQ Qual Code,HONOURS,PQ Class Code,PQ Subject Code 1,PQ Subject Code 2,PQ Subject Code 3\r\n{trn},firstname,lastname,{person.DateOfBirth.ToString("dd/MM/yyyy")},49, 04/04/2014,,,,,,,,,,,,,,,,,,,,,,,,\r\n";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var stream = new MemoryStream(csvBytes);
        var reader = new StreamReader(stream);

        // Act
        var integrationTransactionId = await WithJob(job => job.ImportAsync("INVALID_FILETYPE.csv", reader));

        // Assert
        await DbContextFactory.WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).FirstOrDefaultAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Null(integrationTransactionId);
            Assert.Null(it);
            Mock.Get(Fixture.Services.GetRequiredService<ILogger<EwcWalesImportJob>>()).Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Import filename must begin with IND or QTS")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        });
    }
}

public class EwcWalesImportJobFixture : IAsyncLifetime, IDisposable
{
    public EwcWalesImportJobFixture()
    {
        var services = new ServiceCollection();
        CoreFixture.AddCoreServices(services);
        services.AddSingleton<BlobServiceClient>(Mock.Of<BlobServiceClient>());
        services.AddSingleton(Mock.Of<ILogger<EwcWalesImportJob>>());
        services.AddSingleton(Mock.Of<ILogger<InductionImporter>>());
        services.AddTransient<EwcWalesImportJob>();
        services.AddTransient<InductionImporter>();
        services.AddTransient<QtsImporter>();
        services.AddWebhookMessageFactory();
        services.AddMemoryCache();
        Services = services.BuildServiceProvider();
    }

    public TestableClock Clock => Services.GetRequiredService<TestableClock>();

    public IDbContextFactory<TrsDbContext> DbContextFactory => Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public DbHelper DbHelper => Services.GetRequiredService<DbHelper>();

    public IServiceProvider Services { get; }

    public TestData TestData => Services.GetRequiredService<TestData>();

    async ValueTask IAsyncLifetime.InitializeAsync() => await DbContextFactory.WithDbContextAsync(dbContext => dbContext.Events.ExecuteDeleteAsync());

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    void IDisposable.Dispose() => (Services as IDisposable)?.Dispose();
}
