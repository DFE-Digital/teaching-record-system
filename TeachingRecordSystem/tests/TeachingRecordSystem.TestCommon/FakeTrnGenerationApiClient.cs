using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.TestCommon;

public class FakeTrnGenerationApiClient(FakeTrnGenerator genrator) : ITrnGenerationApiClient
{
    public Task<string> GenerateTrnAsync()
    {
        var trn = genrator.GenerateTrn();
        return Task.FromResult(trn);
    }
}
