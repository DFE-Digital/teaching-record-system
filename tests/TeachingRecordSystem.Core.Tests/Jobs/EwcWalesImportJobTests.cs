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
}
