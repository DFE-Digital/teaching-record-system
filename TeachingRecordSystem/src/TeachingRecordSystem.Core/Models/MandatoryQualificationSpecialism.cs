using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum MandatoryQualificationSpecialism
{
    [MandatoryQualificationInfo(isValidForNewRecord: false), Display(Name = "Auditory")]
    Auditory = 0,

    [MandatoryQualificationInfo, Display(Name = "Deaf education")]
    DeafEducation = 1,

    [MandatoryQualificationInfo, Display(Name = "Hearing")]
    Hearing = 2,

    [MandatoryQualificationInfo, Display(Name = "Multi-Sensory")]
    MultiSensory = 3,

    [MandatoryQualificationInfo, Display(Name = "N/A")]
    NotApplicable = 4,

    [MandatoryQualificationInfo, Display(Name = "Visual")]
    Visual = 5,
}

public static class MandatoryQualificationSpecialismExtensions
{
    public static bool IsValidForNewRecord(this MandatoryQualificationSpecialism specialism) =>
        specialism.GetType()
            .GetMember(specialism.ToString())
            .Single()
            .GetCustomAttribute<MandatoryQualificationInfoAttribute>()?
            .IsValidForNewRecord == true;
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class MandatoryQualificationInfoAttribute(bool isValidForNewRecord = true) : Attribute
{
    public bool IsValidForNewRecord => isValidForNewRecord;
}
