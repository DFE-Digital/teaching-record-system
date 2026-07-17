namespace TeachingRecordSystem.Core.Models.SupportTasks;

/// Where the value of a person attribute comes from when a TRN request is resolved to an existing record.
public enum PersonAttributeSource
{
    ExistingRecord = 0,
    TrnRequest = 1
}

/// <summary>
/// The chosen source for each of a person's attributes when resolving a TRN request to an existing record.
///
/// A null source means no choice was made, which is what happens when the two values don't differ, or when
/// the journey doesn't offer a choice for that attribute at all. Only <see cref="PersonAttributeSource.TrnRequest"/>
/// updates the record, so any other source — including none — keeps the existing value.
///
/// The Teachers' Pensions journey has no email address to resolve and leaves <see cref="EmailAddress"/> unset.
/// </summary>
public record PersonAttributeSources
{
    public PersonAttributeSource? FirstName { get; init; }
    public PersonAttributeSource? MiddleName { get; init; }
    public PersonAttributeSource? LastName { get; init; }
    public PersonAttributeSource? DateOfBirth { get; init; }
    public PersonAttributeSource? EmailAddress { get; init; }
    public PersonAttributeSource? NationalInsuranceNumber { get; init; }
    public PersonAttributeSource? Gender { get; init; }

    /// The attributes to be taken from the TRN request, i.e. the ones whose value on the record changes.
    public IReadOnlyCollection<PersonMatchedAttribute> GetAttributesToUpdate() => Impl().AsReadOnly();

    private IEnumerable<PersonMatchedAttribute> Impl()
    {
        if (FirstName is PersonAttributeSource.TrnRequest)
        {
            yield return PersonMatchedAttribute.FirstName;
        }

        if (MiddleName is PersonAttributeSource.TrnRequest)
        {
            yield return PersonMatchedAttribute.MiddleName;
        }

        if (LastName is PersonAttributeSource.TrnRequest)
        {
            yield return PersonMatchedAttribute.LastName;
        }

        if (DateOfBirth is PersonAttributeSource.TrnRequest)
        {
            yield return PersonMatchedAttribute.DateOfBirth;
        }

        if (EmailAddress is PersonAttributeSource.TrnRequest)
        {
            yield return PersonMatchedAttribute.EmailAddress;
        }

        if (NationalInsuranceNumber is PersonAttributeSource.TrnRequest)
        {
            yield return PersonMatchedAttribute.NationalInsuranceNumber;
        }

        if (Gender is PersonAttributeSource.TrnRequest)
        {
            yield return PersonMatchedAttribute.Gender;
        }
    }
}
