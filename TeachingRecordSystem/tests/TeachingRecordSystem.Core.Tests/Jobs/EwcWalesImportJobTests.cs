using System.Text;
using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public partial class EwcWalesImportJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    private Mock<ILogger<EwcWalesImportJob>> Logger { get; } = new();

    [Theory]
    [InlineData("IND", EwcWalesImportFileType.Induction)]
    [InlineData("QTS", EwcWalesImportFileType.Qualification)]
    [InlineData("", null)]
    public async Task EwcWalesImportJob_GetImportFileType_ReturnsExpected(string filename, EwcWalesImportFileType? importType)
    {
        EwcWalesImportFileType? type = null;
        await WithServiceAsync<EwcWalesImportJob>(
            job => { job.TryGetImportFileType(filename, out type); return Task.CompletedTask; },
            Logger.Object);
        Assert.Equal(importType, type);
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
        var integrationTransactionId = await WithServiceAsync<EwcWalesImportJob, long?>(
            job => job.ImportAsync("INVALID_FILETYPE.csv", reader),
            Logger.Object);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var it = await dbContext.IntegrationTransactions.Include(x => x.IntegrationTransactionRecords).FirstOrDefaultAsync(x => x.IntegrationTransactionId == integrationTransactionId);
            Assert.Null(integrationTransactionId);
            Assert.Null(it);
            Logger.Verify(
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
