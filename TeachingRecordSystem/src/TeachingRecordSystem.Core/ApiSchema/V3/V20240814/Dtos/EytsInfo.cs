namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

public record EytsInfo
{
    public required DateOnly Awarded { get; init; }
    public required string StatusDescription { get; init; }
}
