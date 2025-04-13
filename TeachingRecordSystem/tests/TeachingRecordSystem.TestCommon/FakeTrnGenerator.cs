using TeachingRecordSystem.Core.Services.TrnGeneration;

namespace TeachingRecordSystem.TestCommon;

public class FakeTrnGenerator : ITrnGenerator
{
    private int _lastTrn = 4000000;

    public string GenerateTrn() => Interlocked.Increment(ref _lastTrn).ToString();

    Task<string> ITrnGenerator.GenerateTrnAsync() => Task.FromResult(GenerateTrn());
}
