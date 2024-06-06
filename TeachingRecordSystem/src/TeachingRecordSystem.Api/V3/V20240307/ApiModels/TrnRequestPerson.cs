using AutoMapper.Configuration.Annotations;

namespace TeachingRecordSystem.Api.V3.V20240307.ApiModels;

[AutoMap(typeof(Core.SharedModels.TrnRequestInfoPerson))]
public record TrnRequestPerson
{
    public required string FirstName { get; init; }
    public string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    [SourceMember("EmailAddress")]
    public string? Email { get; init; }
    public string? NationalInsuranceNumber { get; init; }
}
