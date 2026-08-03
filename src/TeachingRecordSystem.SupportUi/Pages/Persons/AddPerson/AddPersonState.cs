using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

public class AddPersonState
{
    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public AddPersonFieldState<EmailAddress> EmailAddress { get; set; } = new("", null);
    public AddPersonFieldState<NationalInsuranceNumber> NationalInsuranceNumber { get; set; } = new("", null);
    public Gender? Gender { get; set; }

    public PersonCreateReason? Reason { get; set; }
    public string? ReasonDetail { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();

    public ProvideMoreInformationOption? ProvideAdditionalInformation { get; set; }

    public string? AdditionalInformation { get; set; }
}
