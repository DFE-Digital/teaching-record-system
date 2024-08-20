namespace TeachingRecordSystem.Api.V3.V20240814.ApiModels;

[AutoMap(typeof(Core.SharedModels.EytsInfo))]
public record EytsInfo
{
    public required DateOnly Awarded { get; init; }
    public required string StatusDescription { get; init; }
}
