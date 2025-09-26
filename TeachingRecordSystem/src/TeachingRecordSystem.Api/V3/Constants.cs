namespace TeachingRecordSystem.Api.V3;

public static class Constants
{
    public static IReadOnlyCollection<string> LegacyExposableSanctionCodes { get; } =
    [
        "A18",
        "A7",
        "A3",
        "C2",
        "T2",
        "B3",
        "C1",
        "T3",
        "T5",
        "T4",
        "T1",
        "A25B",
        "A25A",
        "A21B",
        "A21A",
        "A5B",
        "A5A",
        "A1B",
        "A1A",
        "C3",
        "T6",
        "T7",
        "A20",
        "A19",
        "A14",
        "A6",
        "A13",
        "A2",
        "A24",
        "A23"
    ];

    public static IReadOnlyCollection<string> LegacyProhibitionSanctionCodes { get; } =
    [
        "G1",
        "B1",
        "G2",
        "B6",
        "T2",
        "B3",
        "B5",
        "T3",
        "T5",
        "T4",
        "T1",
        "A25B",
        "A25A",
        "A21B",
        "A21A",
        "A5B",
        "A5A",
        "A1B",
        "A1A"
    ];
}
