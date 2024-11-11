namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record QtlsResult
{
    public required DateOnly? QtsDate { get; init; }
    public required string Trn { get; init; }
}
