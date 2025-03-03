using System.Reflection;

namespace TeachingRecordSystem.Core;

public enum FieldRequirementLevel
{
    NotAppplicable,
    Required,
    Optional
}

public enum ProfessionalStatusStatus
{
    [ProfessionalStatusStatusInfo("In training",
        startDate: FieldRequirementLevel.Required,
        endDate: FieldRequirementLevel.Required,
        awardDate: FieldRequirementLevel.NotAppplicable,
        inductionExemption: FieldRequirementLevel.NotAppplicable,
        trainingProvider: FieldRequirementLevel.Optional,
        degreeType: FieldRequirementLevel.Optional,
        country: FieldRequirementLevel.Optional,
        ageRange: FieldRequirementLevel.Optional,
        subjects: FieldRequirementLevel.Optional)]
    InTraining = 0,
    [ProfessionalStatusStatusInfo("Awarded",
        startDate: FieldRequirementLevel.Required,
        endDate: FieldRequirementLevel.Required,
        awardDate: FieldRequirementLevel.Required,
        inductionExemption: FieldRequirementLevel.Required,
        trainingProvider: FieldRequirementLevel.Optional,
        degreeType: FieldRequirementLevel.Optional,
        country: FieldRequirementLevel.Optional,
        ageRange: FieldRequirementLevel.Optional,
        subjects: FieldRequirementLevel.Optional)]
    Awarded = 1,
    [ProfessionalStatusStatusInfo("Deferred",
        startDate: FieldRequirementLevel.NotAppplicable,
        endDate: FieldRequirementLevel.NotAppplicable,
        awardDate: FieldRequirementLevel.NotAppplicable,
        inductionExemption: FieldRequirementLevel.NotAppplicable,
        trainingProvider: FieldRequirementLevel.NotAppplicable,
        degreeType: FieldRequirementLevel.NotAppplicable,
        country: FieldRequirementLevel.NotAppplicable,
        ageRange: FieldRequirementLevel.NotAppplicable,
        subjects: FieldRequirementLevel.NotAppplicable)]
    Deferred = 2,
    [ProfessionalStatusStatusInfo("Deferred for skills tests",
        startDate: FieldRequirementLevel.NotAppplicable,
        endDate: FieldRequirementLevel.NotAppplicable,
        awardDate: FieldRequirementLevel.NotAppplicable,
        inductionExemption: FieldRequirementLevel.NotAppplicable,
        trainingProvider: FieldRequirementLevel.NotAppplicable,
        degreeType: FieldRequirementLevel.NotAppplicable,
        country: FieldRequirementLevel.NotAppplicable,
        ageRange: FieldRequirementLevel.NotAppplicable,
        subjects: FieldRequirementLevel.NotAppplicable)]
    DeferredForSkillsTest = 3,
    [ProfessionalStatusStatusInfo("Failed",
        startDate: FieldRequirementLevel.NotAppplicable,
        endDate: FieldRequirementLevel.NotAppplicable,
        awardDate: FieldRequirementLevel.NotAppplicable,
        inductionExemption: FieldRequirementLevel.NotAppplicable,
        trainingProvider: FieldRequirementLevel.NotAppplicable,
        degreeType: FieldRequirementLevel.NotAppplicable,
        country: FieldRequirementLevel.NotAppplicable,
        ageRange: FieldRequirementLevel.NotAppplicable,
        subjects: FieldRequirementLevel.NotAppplicable)]
    Failed = 4,
    [ProfessionalStatusStatusInfo("Withdrawn",
        startDate: FieldRequirementLevel.NotAppplicable,
        endDate: FieldRequirementLevel.NotAppplicable,
        awardDate: FieldRequirementLevel.NotAppplicable,
        inductionExemption: FieldRequirementLevel.NotAppplicable,
        trainingProvider: FieldRequirementLevel.NotAppplicable,
        degreeType: FieldRequirementLevel.NotAppplicable,
        country: FieldRequirementLevel.NotAppplicable,
        ageRange: FieldRequirementLevel.NotAppplicable,
        subjects: FieldRequirementLevel.NotAppplicable)]
    Withdrawn = 5,
    [ProfessionalStatusStatusInfo("Under assessment",
        startDate: FieldRequirementLevel.Required,
        endDate: FieldRequirementLevel.Required,
        awardDate: FieldRequirementLevel.NotAppplicable,
        inductionExemption: FieldRequirementLevel.NotAppplicable,
        trainingProvider: FieldRequirementLevel.Optional,
        degreeType: FieldRequirementLevel.Optional,
        country: FieldRequirementLevel.Optional,
        ageRange: FieldRequirementLevel.Optional,
        subjects: FieldRequirementLevel.Optional)]
    UnderAssessment = 6,
    [ProfessionalStatusStatusInfo("Approved",
        startDate: FieldRequirementLevel.Required,
        endDate: FieldRequirementLevel.Required,
        awardDate: FieldRequirementLevel.Required,
        inductionExemption: FieldRequirementLevel.Required,
        trainingProvider: FieldRequirementLevel.Optional,
        degreeType: FieldRequirementLevel.Optional,
        country: FieldRequirementLevel.Optional,
        ageRange: FieldRequirementLevel.Optional,
        subjects: FieldRequirementLevel.Optional)]
    Approved = 7
}

public static class ProfessionalStatusStatusRegistry
{
    private static readonly IReadOnlyDictionary<ProfessionalStatusStatus, ProfessionalStatusStatusInfo> _info =
        Enum.GetValues<ProfessionalStatusStatus>().ToDictionary(s => s, GetInfo);

    public static IReadOnlyCollection<ProfessionalStatusStatusInfo> All => _info.Values.ToArray();

    public static string GetName(this ProfessionalStatusStatus status) => _info[status].Name;

    public static string GetTitle(this ProfessionalStatusStatus status) => _info[status].Title;

    public static FieldRequirementLevel GetStartDateRequirement(this ProfessionalStatusStatus status) => _info[status].StartDate;

    public static FieldRequirementLevel GetEndDateRequirement(this ProfessionalStatusStatus status) => _info[status].EndDate;

    public static FieldRequirementLevel GetAwardDateRequirement(this ProfessionalStatusStatus status) => _info[status].AwardDate;

    public static FieldRequirementLevel GetInductionExemptionRequirement(this ProfessionalStatusStatus status) => _info[status].InductionExemption;

    public static FieldRequirementLevel GetTrainingProviderRequirement(this ProfessionalStatusStatus status) => _info[status].TrainingProvider;

    public static FieldRequirementLevel GetDegreeTypeRequirement(this ProfessionalStatusStatus status) => _info[status].DegreeType;

    public static FieldRequirementLevel GetCountryRequirement(this ProfessionalStatusStatus status) => _info[status].Country;

    public static FieldRequirementLevel GetAgeRangeRequirement(this ProfessionalStatusStatus status) => _info[status].AgeRange;

    public static FieldRequirementLevel GetSubjectsRequirement(this ProfessionalStatusStatus status) => _info[status].Subjects;

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
    FieldRequirementLevel StartDate,
    FieldRequirementLevel EndDate,
    FieldRequirementLevel AwardDate,
    FieldRequirementLevel InductionExemption,
    FieldRequirementLevel TrainingProvider,
    FieldRequirementLevel DegreeType,
    FieldRequirementLevel Country,
    FieldRequirementLevel AgeRange,
    FieldRequirementLevel Subjects)
{
    public string Title => Name[..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class ProfessionalStatusStatusInfoAttribute(
        string name,
        FieldRequirementLevel startDate,
        FieldRequirementLevel endDate,
        FieldRequirementLevel awardDate,
        FieldRequirementLevel inductionExemption,
        FieldRequirementLevel trainingProvider,
        FieldRequirementLevel degreeType,
        FieldRequirementLevel country,
        FieldRequirementLevel ageRange,
        FieldRequirementLevel subjects
    ) : Attribute
{
    public string Name => name;
    public FieldRequirementLevel StartDate => startDate;
    public FieldRequirementLevel EndDate => endDate;
    public FieldRequirementLevel AwardDate => awardDate;
    public FieldRequirementLevel InductionExemption => inductionExemption;
    public FieldRequirementLevel TrainingProvider => trainingProvider;
    public FieldRequirementLevel DegreeType => degreeType;
    public FieldRequirementLevel Country => country;
    public FieldRequirementLevel AgeRange => ageRange;
    public FieldRequirementLevel Subjects => subjects;
}
