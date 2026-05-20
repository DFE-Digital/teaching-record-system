using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.V3.V20240412.Responses;

[AutoMap(typeof(CreateDateOfBirthChangeRequestResult))]
public record CreateDateOfBirthChangeResponse
{
    public required string CaseNumber { get; init; }
}
