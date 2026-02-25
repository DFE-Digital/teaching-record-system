using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core.Events;

public record OneLoginUserUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => IEvent.CoalescePersonIds(OneLoginUser.PersonId, OldOneLoginUser.PersonId);
    public string[] OneLoginUserSubjects => [OneLoginUser.Subject];
    [JsonIgnore]
    public Guid? PersonId => OneLoginUser.PersonId;
    public required EventModels.OneLoginUser OneLoginUser { get; init; }
    public required EventModels.OneLoginUser OldOneLoginUser { get; init; }
    public required OneLoginUserUpdatedEventChanges Changes { get; init; }

    public static OneLoginUserUpdatedEventChanges GetChanges(
        EventModels.OneLoginUser oldOneLoginUser,
        EventModels.OneLoginUser newOneLoginUser)
    {
        var verifiedNamesChanged = oldOneLoginUser.VerifiedNames is not null && newOneLoginUser.VerifiedNames is not null
            ? !oldOneLoginUser.VerifiedNames.SequenceEqual(newOneLoginUser.VerifiedNames, new StringArrayEqualityComparer())
            : oldOneLoginUser.VerifiedNames != newOneLoginUser.VerifiedNames;

        var verifiedDatesOfBirthChanged = oldOneLoginUser.VerifiedDatesOfBirth is not null && newOneLoginUser.VerifiedDatesOfBirth is not null
            ? !oldOneLoginUser.VerifiedDatesOfBirth.SequenceEqual(newOneLoginUser.VerifiedDatesOfBirth)
            : oldOneLoginUser.VerifiedDatesOfBirth != newOneLoginUser.VerifiedDatesOfBirth;

        var matchedAttributesChanged = oldOneLoginUser.MatchedAttributes is not null && newOneLoginUser.MatchedAttributes is not null
            ? !oldOneLoginUser.MatchedAttributes.SequenceEqual(newOneLoginUser.MatchedAttributes, new MatchedAttributeEqualityComparer())
            : oldOneLoginUser.MatchedAttributes != newOneLoginUser.MatchedAttributes;

        return OneLoginUserUpdatedEventChanges.None |
            (oldOneLoginUser.EmailAddress != newOneLoginUser.EmailAddress ? OneLoginUserUpdatedEventChanges.EmailAddress : OneLoginUserUpdatedEventChanges.None) |
            (oldOneLoginUser.PersonId != newOneLoginUser.PersonId ? OneLoginUserUpdatedEventChanges.PersonId : OneLoginUserUpdatedEventChanges.None) |
            (oldOneLoginUser.VerifiedOn != newOneLoginUser.VerifiedOn ? OneLoginUserUpdatedEventChanges.VerifiedOn : OneLoginUserUpdatedEventChanges.None) |
            (oldOneLoginUser.VerificationRoute != newOneLoginUser.VerificationRoute ? OneLoginUserUpdatedEventChanges.VerificationRoute : OneLoginUserUpdatedEventChanges.None) |
            (verifiedNamesChanged ? OneLoginUserUpdatedEventChanges.VerifiedNames : OneLoginUserUpdatedEventChanges.None) |
            (verifiedDatesOfBirthChanged ? OneLoginUserUpdatedEventChanges.VerifiedDatesOfBirth : OneLoginUserUpdatedEventChanges.None) |
            (oldOneLoginUser.MatchedOn != newOneLoginUser.MatchedOn ? OneLoginUserUpdatedEventChanges.MatchedOn : OneLoginUserUpdatedEventChanges.None) |
            (oldOneLoginUser.MatchRoute != newOneLoginUser.MatchRoute ? OneLoginUserUpdatedEventChanges.MatchRoute : OneLoginUserUpdatedEventChanges.None) |
            (matchedAttributesChanged ? OneLoginUserUpdatedEventChanges.MatchedAttributes : OneLoginUserUpdatedEventChanges.None) |
            (oldOneLoginUser.VerifiedByApplicationUserId != newOneLoginUser.VerifiedByApplicationUserId ? OneLoginUserUpdatedEventChanges.VerifiedByApplicationUserId : OneLoginUserUpdatedEventChanges.None);
    }

    private class MatchedAttributeEqualityComparer : IEqualityComparer<KeyValuePair<PersonMatchedAttribute, string>>
    {
        public bool Equals(KeyValuePair<PersonMatchedAttribute, string> x, KeyValuePair<PersonMatchedAttribute, string> y)
        {
            return x.Key == y.Key && x.Value == y.Value;
        }

        public int GetHashCode(KeyValuePair<PersonMatchedAttribute, string> obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + obj.Key.GetHashCode();
                hash = hash * 23 + (obj.Value?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }

    private class StringArrayEqualityComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[]? x, string[]? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.SequenceEqual(y);
        }

        public int GetHashCode(string[] obj)
        {
            unchecked
            {
                int hash = 17;

                foreach (var item in obj)
                {
                    hash = hash * 23 + (item?.GetHashCode() ?? 0);
                }

                return hash;
            }
        }
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
