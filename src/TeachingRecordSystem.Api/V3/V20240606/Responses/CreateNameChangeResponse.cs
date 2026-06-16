using TeachingRecordSystem.Api.V3.Operations;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

public record CreateNameChangeResponse
{
    public required string CaseNumber { get; init; }

    public static CreateNameChangeResponse Create(CreateNameChangeRequestResult source) => new()
    {
        CaseNumber = source.CaseNumber
    };
}
