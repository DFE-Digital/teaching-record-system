using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record ResolveTrnRequestOptions
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required string SupportTaskReference { get; init; }

    /// The record the request resolves to, or null to resolve it with a newly created record
    public required Guid? PersonId { get; init; }

    /// Where each of the record's attributes takes its value from; ignored when creating a new record,
    /// which takes every value from the request
    public PersonAttributeSources AttributeSources { get; init; } = new();

    public string? Comments { get; init; }
}
