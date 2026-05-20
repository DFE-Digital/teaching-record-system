namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record QtlsResult
{
    public required DateOnly? QtsDate { get; init; }
    public required string Trn { get; init; }
}
