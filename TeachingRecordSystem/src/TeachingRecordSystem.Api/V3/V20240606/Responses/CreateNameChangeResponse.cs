using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

public record CreateNameChangeResponse
{
    public required string CaseNumber { get; init; }

    public static CreateNameChangeResponse FromModel(CreateNameChangeRequestResult r) => new() { CaseNumber = r.CaseNumber };
}
