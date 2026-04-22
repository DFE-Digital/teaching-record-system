using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

public record CreateDateOfBirthChangeResponse
{
    public required string CaseNumber { get; init; }

    public static CreateDateOfBirthChangeResponse FromModel(CreateDateOfBirthChangeRequestResult r) => new() { CaseNumber = r.CaseNumber };
}
