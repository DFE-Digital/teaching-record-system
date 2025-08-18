
namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

public class MergeState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.MergePerson,
        typeof(MergeState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }
    public string? PersonATrn { get; set; }
    public string? PersonBTrn { get; set; }
    public Guid? PersonAId { get; set; }
    public Guid? PersonBId { get; set; }
    public Guid? PrimaryPersonId { get; set; }
    public bool PersonAttributeSourcesSet { get; set; }
    public PersonAttributeSource? FirstNameSource { get; set; }
    public PersonAttributeSource? MiddleNameSource { get; set; }
    public PersonAttributeSource? LastNameSource { get; set; }
    public PersonAttributeSource? DateOfBirthSource { get; set; }
    public PersonAttributeSource? EmailAddressSource { get; set; }
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    public PersonAttributeSource? GenderSource { get; set; }
    public bool? UploadEvidence { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    public string? Comments { get; set; }

    public async Task EnsureInitializedAsync(Guid personAId, Func<Task<string?>> getPersonATrn)
    {
        if (Initialized)
        {
            return;
        }

        PersonAId = personAId;
        PersonATrn = await getPersonATrn();
        Initialized = true;
    }
}
