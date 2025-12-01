using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.AddPerson;

public class AddPersonStateBuilder
{
    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }

    public PersonCreateReason? Reason { get; set; }
    public string? ReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }

    private bool Initialized { get; set; }

    public AddPersonStateBuilder WithInitializedState()
    {
        Initialized = true;

        return this;
    }

    public AddPersonStateBuilder WithName(string? firstName, string? middleName, string? lastName)
    {
        FirstName = firstName ?? "";
        MiddleName = middleName ?? "";
        LastName = lastName ?? "";
        return this;
    }

    public AddPersonStateBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        return this;
    }

    public AddPersonStateBuilder WithEmail(string? emailAddress)
    {
        EmailAddress = emailAddress;
        return this;
    }

    public AddPersonStateBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
    {
        NationalInsuranceNumber = nationalInsuranceNumber;
        return this;
    }

    public AddPersonStateBuilder WithGender(Gender? gender)
    {
        Gender = gender;
        return this;
    }

    public AddPersonStateBuilder WithAddPersonReasonChoice(PersonCreateReason option, string? detailText = null)
    {
        Reason = option;
        ReasonDetail = detailText;
        return this;
    }

    public AddPersonStateBuilder WithUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = "evidence.jpeg", string evidenceFileSizeDescription = "5MB")
    {
        UploadEvidence = uploadEvidence;
        if (evidenceFileId is Guid id)
        {
            UploadedEvidenceFile = new()
            {
                FileId = id,
                FileName = evidenceFileName ?? "evidence.jpeg",
                FileSizeDescription = evidenceFileSizeDescription ?? "5MB"
            };
        }
        return this;
    }

    public AddPersonState Build()
    {
        return new AddPersonState
        {
            FirstName = FirstName,
            MiddleName = MiddleName,
            LastName = LastName,
            DateOfBirth = DateOfBirth,
            EmailAddress = AddPersonFieldState<EmailAddress>.FromRawValue(EmailAddress),
            NationalInsuranceNumber = AddPersonFieldState<NationalInsuranceNumber>.FromRawValue(NationalInsuranceNumber),
            Gender = Gender,

            Reason = Reason,
            ReasonDetail = ReasonDetail,
            Evidence = new()
            {
                UploadEvidence = UploadEvidence,
                UploadedEvidenceFile = UploadedEvidenceFile,
            },
            Initialized = Initialized
        };
    }
}
