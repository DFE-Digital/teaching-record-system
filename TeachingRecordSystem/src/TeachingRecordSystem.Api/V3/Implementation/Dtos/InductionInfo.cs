namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record InductionInfo
{
    public required InductionStatus Status { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? CompletedDate { get; init; }
    public required IReadOnlyCollection<PostgresModels.InductionExemptionReason> ExemptionReasons { get; init; }

    public static async Task<InductionInfo> CreateAsync(
        PostgresModels.Person person,
        ReferenceDataCache referenceDataCache)
    {
        return new InductionInfo()
        {
            Status = person.InductionStatus,
            StartDate = person.InductionStartDate,
            CompletedDate = person.InductionCompletedDate,
            ExemptionReasons = await person.GetAllInductionExemptionReasonIds()
                .ToAsyncEnumerable()
                .SelectAwait(async id => await referenceDataCache.GetInductionExemptionReasonByIdAsync(id))
                .ToArrayAsync()
        };
    }
}
