using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum RouteToProfessionalStatusStatus
{
    [ProfessionalStatusStatusInfo("In training",
        startAndEndDates: FieldRequirement.Mandatory,
        holdsFrom: FieldRequirement.NotApplicable,
        inductionExemption: FieldRequirement.NotApplicable,
        trainingProvider: FieldRequirement.Optional,
        degreeType: FieldRequirement.Optional,
        country: FieldRequirement.Mandatory,
        ageRange: FieldRequirement.Optional,
        subjects: FieldRequirement.Optional)]
    InTraining = 0,

    [ProfessionalStatusStatusInfo("Holds",
        startAndEndDates: FieldRequirement.Mandatory,
        holdsFrom: FieldRequirement.Mandatory,
        inductionExemption: FieldRequirement.Mandatory,
        trainingProvider: FieldRequirement.Optional,
        degreeType: FieldRequirement.Optional,
        country: FieldRequirement.Mandatory,
        ageRange: FieldRequirement.Optional,
        subjects: FieldRequirement.Optional)]
    Holds = 1,

    [ProfessionalStatusStatusInfo("Deferred",
        startAndEndDates: FieldRequirement.NotApplicable,
        holdsFrom: FieldRequirement.NotApplicable,
        inductionExemption: FieldRequirement.NotApplicable,
        trainingProvider: FieldRequirement.NotApplicable,
        degreeType: FieldRequirement.NotApplicable,
        country: FieldRequirement.Mandatory,
        ageRange: FieldRequirement.NotApplicable,
        subjects: FieldRequirement.NotApplicable)]
    Deferred = 2,

    [ProfessionalStatusStatusInfo("Deferred for skills tests",
        startAndEndDates: FieldRequirement.NotApplicable,
        holdsFrom: FieldRequirement.NotApplicable,
        inductionExemption: FieldRequirement.NotApplicable,
        trainingProvider: FieldRequirement.NotApplicable,
        degreeType: FieldRequirement.NotApplicable,
        country: FieldRequirement.Mandatory,
        ageRange: FieldRequirement.NotApplicable,
        subjects: FieldRequirement.NotApplicable)]
    DeferredForSkillsTest = 3,

    [ProfessionalStatusStatusInfo("Failed",
        startAndEndDates: FieldRequirement.NotApplicable,
        holdsFrom: FieldRequirement.NotApplicable,
        inductionExemption: FieldRequirement.NotApplicable,
        trainingProvider: FieldRequirement.NotApplicable,
        degreeType: FieldRequirement.NotApplicable,
        country: FieldRequirement.Mandatory,
        ageRange: FieldRequirement.NotApplicable,
        subjects: FieldRequirement.NotApplicable)]
    Failed = 4,

    [ProfessionalStatusStatusInfo("Withdrawn",
        startAndEndDates: FieldRequirement.NotApplicable,
        holdsFrom: FieldRequirement.NotApplicable,
        inductionExemption: FieldRequirement.NotApplicable,
        trainingProvider: FieldRequirement.NotApplicable,
        degreeType: FieldRequirement.NotApplicable,
        country: FieldRequirement.Mandatory,
        ageRange: FieldRequirement.NotApplicable,
        subjects: FieldRequirement.NotApplicable)]
    Withdrawn = 5,

    [ProfessionalStatusStatusInfo("Under assessment",
        startAndEndDates: FieldRequirement.Mandatory,
        holdsFrom: FieldRequirement.NotApplicable,
        inductionExemption: FieldRequirement.NotApplicable,
        trainingProvider: FieldRequirement.Optional,
        degreeType: FieldRequirement.Optional,
        country: FieldRequirement.Mandatory,
        ageRange: FieldRequirement.Optional,
        subjects: FieldRequirement.Optional)]
    UnderAssessment = 6
}

public static class ProfessionalStatusStatusRegistry
{
    private static readonly IReadOnlyDictionary<RouteToProfessionalStatusStatus, ProfessionalStatusStatusInfo> _info =
        Enum.GetValues<RouteToProfessionalStatusStatus>().ToDictionary(s => s, GetInfo);

    public static IReadOnlyCollection<ProfessionalStatusStatusInfo> All => _info.Values.OrderBy(s => s.Name).ToArray();

    public static string GetName(this RouteToProfessionalStatusStatus status) => _info[status].Name;

    public static string GetTitle(this RouteToProfessionalStatusStatus status) => _info[status].Title;

    public static FieldRequirement GetStartAndEndDateRequirement(this RouteToProfessionalStatusStatus status) => _info[status].TrainingStartAndEndDateRequired;

    public static FieldRequirement GetHoldsFromRequirement(this RouteToProfessionalStatusStatus status) => _info[status].HoldsFromRequired;

    public static FieldRequirement GetInductionExemptionRequirement(this RouteToProfessionalStatusStatus status) => _info[status].InductionExemptionRequired;

    public static FieldRequirement GetTrainingProviderRequirement(this RouteToProfessionalStatusStatus status) => _info[status].TrainingProviderRequired;

    public static FieldRequirement GetDegreeTypeRequirement(this RouteToProfessionalStatusStatus status) => _info[status].DegreeTypeRequired;

    public static FieldRequirement GetCountryRequirement(this RouteToProfessionalStatusStatus status) => _info[status].TrainingCountryRequired;

    public static FieldRequirement GetAgeSpecialismRequirement(this RouteToProfessionalStatusStatus status) => _info[status].TrainingAgeSpecialismTypeRequired;

    public static FieldRequirement GetSubjectsRequirement(this RouteToProfessionalStatusStatus status) => _info[status].TrainingSubjectsRequired;

    private static ProfessionalStatusStatusInfo GetInfo(RouteToProfessionalStatusStatus status)
    {
        var attr = status.GetType()
               .GetMember(status.ToString())
               .Single()
               .GetCustomAttribute<ProfessionalStatusStatusInfoAttribute>() ??
           throw new Exception($"{nameof(RouteToProfessionalStatusStatus)}.{status} is missing the {nameof(ProfessionalStatusStatusInfoAttribute)} attribute.");

        return new ProfessionalStatusStatusInfo(
            status, attr.Name, attr.StartAndEndDates, attr.HoldsDate, attr.InductionExemption, attr.TrainingProvider, attr.DegreeType, attr.Country, attr.AgeRange, attr.Subjects);
    }
}

public sealed record ProfessionalStatusStatusInfo(
    RouteToProfessionalStatusStatus Value,
    string Name,
    FieldRequirement TrainingStartAndEndDateRequired,
    FieldRequirement HoldsFromRequired,
    FieldRequirement InductionExemptionRequired,
    FieldRequirement TrainingProviderRequired,
    FieldRequirement DegreeTypeRequired,
    FieldRequirement TrainingCountryRequired,
    FieldRequirement TrainingAgeSpecialismTypeRequired,
    FieldRequirement TrainingSubjectsRequired)
{
    public string Title => Name[..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class ProfessionalStatusStatusInfoAttribute(
        string name,
        FieldRequirement startAndEndDates,
        FieldRequirement holdsFrom,
        FieldRequirement inductionExemption,
        FieldRequirement trainingProvider,
        FieldRequirement degreeType,
        FieldRequirement country,
        FieldRequirement ageRange,
        FieldRequirement subjects
    ) : Attribute
{
    public string Name => name;
    public FieldRequirement StartAndEndDates => startAndEndDates;
    public FieldRequirement HoldsDate => holdsFrom;
    public FieldRequirement InductionExemption => inductionExemption;
    public FieldRequirement TrainingProvider => trainingProvider;
    public FieldRequirement DegreeType => degreeType;
    public FieldRequirement Country => country;
    public FieldRequirement AgeRange => ageRange;
    public FieldRequirement Subjects => subjects;
}
