using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.TestCommon;

public class FakeTrnGenerationApiClient(FakeTrnGenerator genrator) : ITrnGenerationApiClient
{
    public Task<string> GenerateTrn()
    {
        var trn = genrator.GenerateTrn();
        return Task.FromResult(trn);
    }
}
