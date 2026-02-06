namespace TeachingRecordSystem.Core.Events;

public record OneLoginUserUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [];
    public required EventModels.OneLoginUser OneLoginUser { get; init; }
    public required EventModels.OneLoginUser OldOneLoginUser { get; init; }
    public required OneLoginUserUpdatedEventChanges Changes { get; init; }

    public static OneLoginUserUpdatedEventChanges GetChanges(
        EventModels.OneLoginUser oldOneLoginUser,
        EventModels.OneLoginUser newOneLoginUser)
    {
        throw new NotImplementedException();
    }
}

[Flags]
public enum OneLoginUserUpdatedEventChanges
{
    None = 0,
    EmailAddress = 1 << 0,
    PersonId = 1 << 1,
    VerifiedOn = 1 << 2,
    VerificationRoute = 1 << 3,
    VerifiedNames = 1 << 4,
    VerifiedDatesOfBirth = 1 << 5,
    MatchedOn = 1 << 6,
    MatchRoute = 1 << 7,
    MatchedAttributes = 1 << 8,
    VerifiedByApplicationUserId = 1 << 9
}
