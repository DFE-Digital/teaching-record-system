using Optional;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateContactQuery : ICrmQuery<Guid>
{
    public required Guid ContactId { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required Contact_GenderCode Gender { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required string ApplicationUserName { get; init; }
    public required string? Trn { get; init; }
    public required string? TrnRequestId { get; init; }
    public required bool AllowPiiUpdates { get; init; }
}
