using TeachingRecordSystem.Api.V3.Operations;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

public record CreateDateOfBirthChangeResponse
{
    public required string CaseNumber { get; init; }

    public static CreateDateOfBirthChangeResponse Create(CreateDateOfBirthChangeRequestResult source) => new()
    {
        CaseNumber = source.CaseNumber
    };
}
