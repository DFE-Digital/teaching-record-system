using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.TestCommon;

public class FakeTrnGenerationApiClient(FakeTrnGenerator genrator) : ITrnGenerator
{
    public Task<string> GenerateTrnAsync()
    {
        var trn = genrator.GenerateTrn();
        return Task.FromResult(trn);
    }
}
