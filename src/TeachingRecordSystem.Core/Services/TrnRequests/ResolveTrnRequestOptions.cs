using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record ResolveTrnRequestOptions
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required string SupportTaskReference { get; init; }

    /// The record the request resolves to, or null to resolve it with a newly created record
    public required Guid? PersonId { get; init; }

    public IReadOnlyCollection<PersonMatchedAttribute> AttributesToUpdate { get; init; } = [];
    public required TrnRequestDataPersonAttributes? ResolvedAttributes { get; init; }
    public required TrnRequestDataPersonAttributes? SelectedPersonAttributes { get; init; }
    public string? Comments { get; init; }
}
