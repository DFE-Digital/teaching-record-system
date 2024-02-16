namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record QtlsInfo
{
    public required DateOnly? QtsDate { get; init; }
    public required string Trn { get; init; }
}
