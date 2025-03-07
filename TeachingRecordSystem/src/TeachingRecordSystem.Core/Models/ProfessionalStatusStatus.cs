using System.Reflection;

namespace TeachingRecordSystem.Core.Models;


public enum ProfessionalStatusStatus
{
    [ProfessionalStatusStatusInfo("In training",
        startDate: FieldRequirement.Mandatory,
        endDate: FieldRequirement.Mandatory,
        awardDate: FieldRequirement.NotRequired,
        inductionExemption: FieldRequirement.NotRequired,
        trainingProvider: FieldRequirement.Optional,
        degreeType: FieldRequirement.Optional,
        country: FieldRequirement.Optional,
        ageRange: FieldRequirement.Optional,
        subjects: FieldRequirement.Optional)]
    InTraining = 0,
    [ProfessionalStatusStatusInfo("Awarded",
        startDate: FieldRequirement.Mandatory,
        endDate: FieldRequirement.Mandatory,
        awardDate: FieldRequirement.Mandatory,
        inductionExemption: FieldRequirement.Mandatory,
        trainingProvider: FieldRequirement.Optional,
        degreeType: FieldRequirement.Optional,
        country: FieldRequirement.Optional,
        ageRange: FieldRequirement.Optional,
        subjects: FieldRequirement.Optional)]
    Awarded = 1,
    [ProfessionalStatusStatusInfo("Deferred",
        startDate: FieldRequirement.NotRequired,
        endDate: FieldRequirement.NotRequired,
        awardDate: FieldRequirement.NotRequired,
        inductionExemption: FieldRequirement.NotRequired,
        trainingProvider: FieldRequirement.NotRequired,
        degreeType: FieldRequirement.NotRequired,
        country: FieldRequirement.NotRequired,
        ageRange: FieldRequirement.NotRequired,
        subjects: FieldRequirement.NotRequired)]
    Deferred = 2,
    [ProfessionalStatusStatusInfo("Deferred for skills tests",
        startDate: FieldRequirement.NotRequired,
        endDate: FieldRequirement.NotRequired,
        awardDate: FieldRequirement.NotRequired,
        inductionExemption: FieldRequirement.NotRequired,
        trainingProvider: FieldRequirement.NotRequired,
        degreeType: FieldRequirement.NotRequired,
        country: FieldRequirement.NotRequired,
        ageRange: FieldRequirement.NotRequired,
        subjects: FieldRequirement.NotRequired)]
    DeferredForSkillsTest = 3,
    [ProfessionalStatusStatusInfo("Failed",
        startDate: FieldRequirement.NotRequired,
        endDate: FieldRequirement.NotRequired,
        awardDate: FieldRequirement.NotRequired,
        inductionExemption: FieldRequirement.NotRequired,
        trainingProvider: FieldRequirement.NotRequired,
        degreeType: FieldRequirement.NotRequired,
        country: FieldRequirement.NotRequired,
        ageRange: FieldRequirement.NotRequired,
        subjects: FieldRequirement.NotRequired)]
    Failed = 4,
    [ProfessionalStatusStatusInfo("Withdrawn",
        startDate: FieldRequirement.NotRequired,
        endDate: FieldRequirement.NotRequired,
        awardDate: FieldRequirement.NotRequired,
        inductionExemption: FieldRequirement.NotRequired,
        trainingProvider: FieldRequirement.NotRequired,
        degreeType: FieldRequirement.NotRequired,
        country: FieldRequirement.NotRequired,
        ageRange: FieldRequirement.NotRequired,
        subjects: FieldRequirement.NotRequired)]
    Withdrawn = 5,
    [ProfessionalStatusStatusInfo("Under assessment",
        startDate: FieldRequirement.Mandatory,
        endDate: FieldRequirement.Mandatory,
        awardDate: FieldRequirement.NotRequired,
        inductionExemption: FieldRequirement.NotRequired,
        trainingProvider: FieldRequirement.Optional,
        degreeType: FieldRequirement.Optional,
        country: FieldRequirement.Optional,
        ageRange: FieldRequirement.Optional,
        subjects: FieldRequirement.Optional)]
    UnderAssessment = 6,
    [ProfessionalStatusStatusInfo("Approved",
        startDate: FieldRequirement.Mandatory,
        endDate: FieldRequirement.Mandatory,
        awardDate: FieldRequirement.Mandatory,
        inductionExemption: FieldRequirement.Mandatory,
        trainingProvider: FieldRequirement.Optional,
        degreeType: FieldRequirement.Optional,
        country: FieldRequirement.Optional,
        ageRange: FieldRequirement.Optional,
        subjects: FieldRequirement.Optional)]
    Approved = 7
}

public static class ProfessionalStatusStatusRegistry
{
    private static readonly IReadOnlyDictionary<ProfessionalStatusStatus, ProfessionalStatusStatusInfo> _info =
        Enum.GetValues<ProfessionalStatusStatus>().ToDictionary(s => s, GetInfo);

    public static IReadOnlyCollection<ProfessionalStatusStatusInfo> All => _info.Values.ToArray();

    public static string GetName(this ProfessionalStatusStatus status) => _info[status].Name;

    public static string GetTitle(this ProfessionalStatusStatus status) => _info[status].Title;

    public static FieldRequirement GetStartDateRequirement(this ProfessionalStatusStatus status) => _info[status].StartDate;

    public static FieldRequirement GetEndDateRequirement(this ProfessionalStatusStatus status) => _info[status].EndDate;

    public static FieldRequirement GetAwardDateRequirement(this ProfessionalStatusStatus status) => _info[status].AwardDate;

    public static FieldRequirement GetInductionExemptionRequirement(this ProfessionalStatusStatus status) => _info[status].InductionExemption;

    public static FieldRequirement GetTrainingProviderRequirement(this ProfessionalStatusStatus status) => _info[status].TrainingProvider;

    public static FieldRequirement GetDegreeTypeRequirement(this ProfessionalStatusStatus status) => _info[status].DegreeType;

    public static FieldRequirement GetCountryRequirement(this ProfessionalStatusStatus status) => _info[status].Country;

    public static FieldRequirement GetAgeRangeRequirement(this ProfessionalStatusStatus status) => _info[status].AgeRange;

    public static FieldRequirement GetSubjectsRequirement(this ProfessionalStatusStatus status) => _info[status].Subjects;

    private static ProfessionalStatusStatusInfo GetInfo(ProfessionalStatusStatus status)
    {
        var attr = status.GetType()
               .GetMember(status.ToString())
               .Single()
               .GetCustomAttribute<ProfessionalStatusStatusInfoAttribute>() ??
           throw new Exception($"{nameof(ProfessionalStatusStatus)}.{status} is missing the {nameof(ProfessionalStatusStatusInfoAttribute)} attribute.");

        return new ProfessionalStatusStatusInfo(
            status, attr.Name, attr.StartDate, attr.EndDate, attr.AwardDate, attr.InductionExemption, attr.TrainingProvider, attr.DegreeType, attr.Country, attr.AgeRange, attr.Subjects);
    }
}

public sealed record ProfessionalStatusStatusInfo(
    ProfessionalStatusStatus Value,
    string Name,
    FieldRequirement StartDate,
    FieldRequirement EndDate,
    FieldRequirement AwardDate,
    FieldRequirement InductionExemption,
    FieldRequirement TrainingProvider,
    FieldRequirement DegreeType,
    FieldRequirement Country,
    FieldRequirement AgeRange,
    FieldRequirement Subjects)
{
    public string Title => Name[..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class ProfessionalStatusStatusInfoAttribute(
        string name,
        FieldRequirement startDate,
        FieldRequirement endDate,
        FieldRequirement awardDate,
        FieldRequirement inductionExemption,
        FieldRequirement trainingProvider,
        FieldRequirement degreeType,
        FieldRequirement country,
        FieldRequirement ageRange,
        FieldRequirement subjects
    ) : Attribute
{
    public string Name => name;
    public FieldRequirement StartDate => startDate;
    public FieldRequirement EndDate => endDate;
    public FieldRequirement AwardDate => awardDate;
    public FieldRequirement InductionExemption => inductionExemption;
    public FieldRequirement TrainingProvider => trainingProvider;
    public FieldRequirement DegreeType => degreeType;
    public FieldRequirement Country => country;
    public FieldRequirement AgeRange => ageRange;
    public FieldRequirement Subjects => subjects;
}
