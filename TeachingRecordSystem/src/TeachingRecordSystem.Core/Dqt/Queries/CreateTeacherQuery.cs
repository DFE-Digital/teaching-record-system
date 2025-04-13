using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateContactQuery : ICrmQuery<Guid>
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string StatedFirstName { get; init; }
    public required string StatedMiddleName { get; init; }
    public required string StatedLastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required Contact_GenderCode Gender { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required IReadOnlyCollection<CreateContactQueryDuplicateReviewTask> ReviewTasks { get; init; }
    public required string ApplicationUserName { get; init; }
    public required string? Trn { get; init; }
    public required string? TrnRequestId { get; init; }
    public required TrnRequestMetadataMessage TrnRequestMetadataMessage { get; init; }
    public required bool AllowPiiUpdates { get; init; }
}

public record CreateContactQueryDuplicateReviewTask
{
    public Guid PotentialDuplicateContactId { get; init; }
    public required string Category { get; init; }
    public required string Subject { get; init; }
    public required string Description { get; init; }
}
