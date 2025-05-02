using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

public class ResolveApiTrnRequestState : IRegisterJourney
{
    public static Guid CreateNewRecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveApiTrnRequest,
        typeof(ResolveApiTrnRequestState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public Guid? PersonId { get; set; }
    public PersonAttributeSource? FirstNameSource { get; set; }
    public PersonAttributeSource? MiddleNameSource { get; set; }
    public PersonAttributeSource? LastNameSource { get; set; }
    public PersonAttributeSource? DateOfBirthSource { get; set; }
    public PersonAttributeSource? EmailAddressSource { get; set; }
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    public string? Comments { get; set; }

    public static IReadOnlyCollection<PersonMatchedAttribute> GetPersonAttributeDifferences(
        TrnRequestMetadata requestMetadata,
        string firstName,
        string middleName,
        string lastName,
        DateOnly? dateOfBirth,
        string? emailAddress,
        string? nationalInsuranceNumber)
    {
        return Impl().AsReadOnly();

        IEnumerable<PersonMatchedAttribute> Impl()
        {
            if (firstName == requestMetadata.FirstName)
            {
                yield return PersonMatchedAttribute.FirstName;
            }

            if (middleName == requestMetadata.MiddleName)
            {
                yield return PersonMatchedAttribute.MiddleName;
            }

            if (lastName == requestMetadata.LastName)
            {
                yield return PersonMatchedAttribute.LastName;
            }

            if (dateOfBirth == requestMetadata.DateOfBirth)
            {
                yield return PersonMatchedAttribute.DateOfBirth;
            }

            if (emailAddress == requestMetadata.EmailAddress)
            {
                yield return PersonMatchedAttribute.EmailAddress;
            }

            if (nationalInsuranceNumber == requestMetadata.NationalInsuranceNumber)
            {
                yield return PersonMatchedAttribute.NationalInsuranceNumber;
            }
        }
    }

    public enum PersonAttributeSource { TrnRequest, ExistingRecord }
}
