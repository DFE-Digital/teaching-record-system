using TeachingRecordSystem.SupportUi.Pages.Persons.Create;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Create;

public class CreateStateBuilder
{
    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalInsuranceNumber { get; set; }

    public CreateReasonOption? CreateReason { get; set; }
    public string? CreateReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }

    private bool Initialized { get; set; }

    public CreateStateBuilder WithInitializedState()
    {
        Initialized = true;

        return this;
    }

    public CreateStateBuilder WithName(string? firstName, string? middleName, string? lastName)
    {
        FirstName = firstName ?? "";
        MiddleName = middleName ?? "";
        LastName = lastName ?? "";
        return this;
    }

    public CreateStateBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        return this;
    }

    public CreateStateBuilder WithEmail(string? emailAddress)
    {
        EmailAddress = emailAddress;
        return this;
    }

    public CreateStateBuilder WithMobileNumber(string? mobileNumber)
    {
        MobileNumber = mobileNumber;
        return this;
    }

    public CreateStateBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
    {
        NationalInsuranceNumber = nationalInsuranceNumber;
        return this;
    }

    public CreateStateBuilder WithCreateReasonChoice(CreateReasonOption option, string? detailText = null)
    {
        CreateReason = option;
        CreateReasonDetail = detailText;
        return this;
    }

    public CreateStateBuilder WithUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = "evidence.jpeg", string evidenceFileSizeDescription = "5MB")
    {
        UploadEvidence = uploadEvidence;
        if (evidenceFileId.HasValue)
        {
            EvidenceFileId = evidenceFileId;
            EvidenceFileName = evidenceFileName;
            EvidenceFileSizeDescription = evidenceFileSizeDescription;
        }
        return this;
    }

    public CreateState Build()
    {
        return new CreateState
        {
            FirstName = FirstName,
            MiddleName = MiddleName,
            LastName = LastName,
            DateOfBirth = DateOfBirth,
            EmailAddress = CreateFieldState<EmailAddress>.FromRawValue(EmailAddress),
            MobileNumber = CreateFieldState<MobileNumber>.FromRawValue(MobileNumber),
            NationalInsuranceNumber = CreateFieldState<NationalInsuranceNumber>.FromRawValue(NationalInsuranceNumber),

            CreateReason = CreateReason,
            CreateReasonDetail = CreateReasonDetail,
            UploadEvidence = UploadEvidence,
            EvidenceFileId = EvidenceFileId,
            EvidenceFileName = EvidenceFileName,
            EvidenceFileSizeDescription = EvidenceFileSizeDescription,

            Initialized = Initialized
        };
    }
}
