namespace TeachingRecordSystem.Api.V3.V20240814.ApiModels;

[AutoMap(typeof(Core.SharedModels.QtsInfo))]
public record QtsInfo
{
    public required DateOnly Awarded { get; init; }
    public required string StatusDescription { get; init; }
}
