using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum MandatoryQualificationSpecialism
{
    [MandatoryQualificationSpecialismInfo(name: "auditory", dqtValue: "Auditory", legacy: true)]
    Auditory = 0,

    [MandatoryQualificationSpecialismInfo(name: "deaf education", dqtValue: "Deaf education", legacy: true)]
    DeafEducation = 1,

    [MandatoryQualificationSpecialismInfo(name: "hearing", dqtValue: "Hearing")]
    Hearing = 2,

    [MandatoryQualificationSpecialismInfo(name: "multi-sensory", dqtValue: "Multi-Sensory")]
    MultiSensory = 3,

    [MandatoryQualificationSpecialismInfo(name: "N/A", dqtValue: "N/A", legacy: true)]
    NotApplicable = 4,

    [MandatoryQualificationSpecialismInfo(name: "visual", dqtValue: "Visual")]
    Visual = 5,
}

public static class MandatoryQualificationSpecialismRegistry
{
    private static readonly IReadOnlyDictionary<MandatoryQualificationSpecialism, MandatoryQualificationSpecialismInfo> _info =
        Enum.GetValues<MandatoryQualificationSpecialism>().ToDictionary(s => s, s => GetInfo(s));

    public static IReadOnlyCollection<MandatoryQualificationSpecialismInfo> GetAll(bool includeLegacy) =>
        _info.Values.Where(i => includeLegacy || !i.Legacy).OrderBy(s => s.Title).ToArray();

    public static string GetName(this MandatoryQualificationSpecialism specialism) => _info[specialism].Name;

    public static string GetTitle(this MandatoryQualificationSpecialism specialism) => _info[specialism].Title;

    public static MandatoryQualificationSpecialism GetByDqtValue(string dqtValue) =>
        _info.Single(i => i.Value.DqtValue == dqtValue, $"Failed mapping '{dqtValue}' to {nameof(MandatoryQualificationSpecialism)}.").Key;

    public static string GetDqtValue(this MandatoryQualificationSpecialism specialism) => _info[specialism].DqtValue;

    public static bool IsLegacy(this MandatoryQualificationSpecialism specialism) => _info[specialism].Legacy;

    public static MandatoryQualificationSpecialism ToMandatoryQualificationSpecialism(this dfeta_specialism specialism) =>
        GetByDqtValue(specialism.dfeta_Value);

    public static bool TryMapFromDqtMqEstablishment(string mqestablishmentValue, string dqtValue, [NotNullWhen(true)] out MandatoryQualificationSpecialism? specialism)
    {
        switch ((mqestablishmentValue, dqtValue))
        {
            case ("963", "Hearing"):  // University of Oxford/Oxford Polytechnic
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("957", "Hearing"):  // University of Edinburgh
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("957", "Visual"):  // University of Edinburgh
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("150", "Hearing"):  // Postgraduate Diploma in Deaf Education, University of Manchester, School of Psychological Sciences
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("961", "Deaf education"):  // University of Manchester
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("961", "Hearing"):  // University of Manchester
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("961", "Visual Impairment"):  // University of Manchester
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("210", "Multi_Sensory Impairment"):  // Postgraduate Diploma in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education
                specialism = MandatoryQualificationSpecialism.MultiSensory;
                return true;
            case ("180", "Multi_Sensory Impairment"):  // BPhil in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education
                specialism = MandatoryQualificationSpecialism.MultiSensory;
                return true;
            case ("180", "Visual Impairment"):  // BPhil in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education
                specialism = MandatoryQualificationSpecialism.MultiSensory;
                return true;
            case ("160", "Hearing"):  // BPhil in Education (Special Education: Hearing Impairment), University of Birmingham, School of Education
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("120", "Hearing"):  // Postgraduate Diploma in Education (Special Education: Hearing Impairment), University of Birmingham, School of Education
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("20", "Visual Impairment"):  // BPhil for Teachers of Children with a Visual Impairment, University of Birmingham, School of Education
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("30", "Visual Impairment"):  // Postgraduate Diploma for Teachers of Children with Visual Impairment, University of Birmingham, School of Education
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("955", "Deaf education"):  // University of Birmingham
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("955", "Hearing"):  // University of Birmingham
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("955", "Multi_Sensory Impairment"):  // University of Birmingham
                specialism = MandatoryQualificationSpecialism.MultiSensory;
                return true;
            case ("955", "N/A"):  // University of Birmingham
                specialism = null;
                return false;
            case ("955", "Visual Impairment"):  // University of Birmingham
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("956", "Hearing"):  // University of Cambridge
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("956", "Visual Impairment"):  // University of Cambridge
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("964", "Visual Impairment"):  // Liverpool John Moores University
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("964", "N/A"):  // Liverpool John Moores University
                specialism = MandatoryQualificationSpecialism.NotApplicable;
                return true;
            case ("959", "Hearing"):  // University of Leeds
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("959", "Deaf"):  // University of Leeds
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("959", "N/A"):  // University of Leeds
                specialism = MandatoryQualificationSpecialism.NotApplicable;
                return true;
            case ("962", "Hearing"):  // University of Newcastle-upon-Tyne
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("90", "Visual Impairment"):  // Masters Level: Mandatory Qualification for Teachers of Children with Visual Impairment, University of Plymouth, Faculty of Education, in partnership with the Sensory Consortium
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("965", "Visual Impairment"):  // Plymouth University
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("140", "Hearing"):  // Postgraduate Diploma (Education of Deaf Children), University of Hertfordshire
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("958", "Deaf education"):  // University of Hertfordshire
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("958", "Hearing"):  // University of Hertfordshire
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("958", "N/A"):  // University of Hertfordshire
                specialism = null;
                return false;
            case ("960", "Hearing"):  // University of London
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("960", "Multi_Sensory Impairment"):  // University of London
                specialism = MandatoryQualificationSpecialism.MultiSensory;
                return true;
            case ("960", "Visual Impairment"):  // University of London
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("50", "Visual Impairment"):  // Graduate Diploma in Special and Inclusive Education: Disabilities of Sight, University of London Institute of Education
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            case ("50", "Hearing"):  // Graduate Diploma in Special and Inclusive Education: Disabilities of Sight, University of London Institute of Education
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("951", "Hearing"):  // Bristol Polytechnic
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("954", "Hearing"):  // University College, Swansea
                specialism = MandatoryQualificationSpecialism.Hearing;
                return true;
            case ("954", "Visual Impairment"):  // University College, Swansea
                specialism = MandatoryQualificationSpecialism.Visual;
                return true;
            default:
                specialism = null;
                return false;
        }
    }

    private static MandatoryQualificationSpecialismInfo GetInfo(MandatoryQualificationSpecialism specialism)
    {
        var attr = specialism.GetType()
            .GetMember(specialism.ToString())
            .Single()
            .GetCustomAttribute<MandatoryQualificationSpecialismInfoAttribute>() ??
            throw new Exception($"{nameof(MandatoryQualificationSpecialism)}.{specialism} is missing the {nameof(MandatoryQualificationSpecialismInfoAttribute)} attribute.");

        return new MandatoryQualificationSpecialismInfo(specialism, attr.Name, attr.DqtValue, attr.Legacy);
    }
}

public sealed record MandatoryQualificationSpecialismInfo(MandatoryQualificationSpecialism Value, string Name, string DqtValue, bool Legacy)
{
    public string Title => Name[0..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class MandatoryQualificationSpecialismInfoAttribute(
    string name,
    string dqtValue,
    bool legacy = false) : Attribute
{
    public string Name => name;
    public string DqtValue => dqtValue;
    public bool Legacy => legacy;
}
