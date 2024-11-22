namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240912.Dtos;

public record QtlsResponse
{
    public required DateOnly? QtsDate { get; init; }
    public required string Trn { get; init; }
}
