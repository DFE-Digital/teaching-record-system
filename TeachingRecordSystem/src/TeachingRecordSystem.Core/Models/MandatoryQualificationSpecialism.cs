using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum MandatoryQualificationSpecialism
{
    [MandatoryQualificationSpecialismInfo(name: "auditory", dqtValue: "Auditory", isValidForNewRecord: false)]
    Auditory = 0,

    [MandatoryQualificationSpecialismInfo(name: "deaf education", dqtValue: "Deaf education", isValidForNewRecord: false)]
    DeafEducation = 1,

    [MandatoryQualificationSpecialismInfo(name: "hearing", dqtValue: "Hearing")]
    Hearing = 2,

    [MandatoryQualificationSpecialismInfo(name: "multi-sensory", dqtValue: "Multi-Sensory")]
    MultiSensory = 3,

    [MandatoryQualificationSpecialismInfo(name: "N/A", dqtValue: "N/A", isValidForNewRecord: false)]
    NotApplicable = 4,

    [MandatoryQualificationSpecialismInfo(name: "visual", dqtValue: "Visual")]
    Visual = 5,
}

public static class MandatoryQualificationSpecialismRegistry
{
    private static readonly IReadOnlyDictionary<MandatoryQualificationSpecialism, MandatoryQualificationSpecialismInfo> _info =
        Enum.GetValues<MandatoryQualificationSpecialism>().ToDictionary(s => s, s => GetInfo(s));

    public static IReadOnlyCollection<MandatoryQualificationSpecialismInfo> GetAll(bool forNewRecord = false) =>
        _info.Values.Where(i => i.IsValidForNewRecord || !forNewRecord).ToArray();

    public static string GetName(this MandatoryQualificationSpecialism specialism) => _info[specialism].Name;

    public static string GetTitle(this MandatoryQualificationSpecialism specialism) => _info[specialism].Title;

    public static string GetDqtValue(this MandatoryQualificationSpecialism specialism) => _info[specialism].DqtValue;

    public static bool IsValidForNewRecord(this MandatoryQualificationSpecialism specialism) => _info[specialism].IsValidForNewRecord;

    public static MandatoryQualificationSpecialism ToMandatoryQualificationSpecialism(this dfeta_specialism specialism) =>
        _info.Values.Single(s => s.DqtValue == specialism.dfeta_Value).Value;

    private static MandatoryQualificationSpecialismInfo GetInfo(MandatoryQualificationSpecialism specialism)
    {
        var attr = specialism.GetType()
            .GetMember(specialism.ToString())
            .Single()
            .GetCustomAttribute<MandatoryQualificationSpecialismInfoAttribute>() ??
            throw new Exception($"{nameof(MandatoryQualificationSpecialism)}.{specialism} is missing the {nameof(MandatoryQualificationSpecialismInfoAttribute)} attribute.");

        return new MandatoryQualificationSpecialismInfo(specialism, attr.Name, attr.DqtValue, attr.IsValidForNewRecord);
    }
}

public sealed record MandatoryQualificationSpecialismInfo(MandatoryQualificationSpecialism Value, string Name, string DqtValue, bool IsValidForNewRecord)
{
    public string Title => Name[0..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class MandatoryQualificationSpecialismInfoAttribute(
    string name,
    string dqtValue,
    bool isValidForNewRecord = true) : Attribute
{
    public string Name => name;
    public string DqtValue => dqtValue;
    public bool IsValidForNewRecord => isValidForNewRecord;
}
