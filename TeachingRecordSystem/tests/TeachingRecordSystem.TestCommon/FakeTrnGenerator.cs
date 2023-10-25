namespace TeachingRecordSystem.TestCommon;

public class FakeTrnGenerator
{
    private int _lastTrn = 4000000;

    public string GenerateTrn() => Interlocked.Increment(ref _lastTrn).ToString();
}
