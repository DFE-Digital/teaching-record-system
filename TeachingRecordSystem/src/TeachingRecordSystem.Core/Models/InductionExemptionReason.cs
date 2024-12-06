namespace TeachingRecordSystem.Core.Models;

[Flags]
public enum InductionExemptionReasons
{
    None = 0,
    SomethingMadeUpForNow = 1 << 0
}
