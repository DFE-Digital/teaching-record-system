#nullable disable
using System.ComponentModel;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.ApiModels;

public enum IttProgrammeType
{
    [Description("Apprenticeship")]
    Apprenticeship = 389040019,

    [Description("Assessment Only Route")]
    AssessmentOnlyRoute = 389040010,

    [Description("Core")]
    Core = 389040000,

    [Description("Core Flexible")]
    CoreFlexible = 389040011,

    [Description("EYITT - Assessment Only")]
    EYITTAssessmentOnly = 389040014,

    [Description("EYITT - Graduate Employment Based")]
    EYITTGraduateEmploymentBased = 389040016,

    [Description("EYITT - Graduate Entry")]
    EYITTGraduateEntry = 389040015,

    [Description("EYITT - School Direct (Early Years)")]
    EYITTSchoolDirectEarlyYears = 389040018,

    [Description("EYITT - Undergraduate")]
    EYITTUndergraduate = 389040017,

    [Description("Future Teaching Scholars")]
    FutureTeachingScholars = 389040020,

    [Description("Graduate Teacher Programme")]
    GraduateTeacherProgramme = 389040009,

    [Description("HEI")]
    HEI = 389040001,

    [Description("Licensed Teacher Programme")]
    LicensedTeacherProgramme = 389040012,

    [Description("Overseas Trained Teacher Programme")]
    OverseasTrainedTeacherProgramme = 389040007,

    [Description("Registered Teacher Programme")]
    RegisteredTeacherProgramme = 389040008,

    [Description("School Direct training programme")]
    SchoolDirectTrainingProgramme = 389040004,

    [Description("School Direct training programme (Salaried)")]
    SchoolDirectTrainingProgrammeSalaried = 389040003,

    [Description("School Direct training programme (Self funded)")]
    SchoolDirectTrainingProgrammeSelfFunded = 389040002,

    [Description("Teach First Programme")]
    TeachFirstProgramme = 389040006,

    [Description("Teach First Programme (CC)")]
    TeachFirstProgrammeCC = 389040005,

    [Description("Undergraduate Opt In")]
    UndergraduateOptIn = 389040013,

    [Description("Provider-led (postgrad)")]
    ProviderLedPostgrad = 389040021,

    [Description("Provider-led (undergrad)")]
    ProviderLedUndergrad = 389040022,

    [Description("International qualified teacher status")]
    InternationalQualifiedTeacherStatus = 389040023,

    [Description("High potential ITT")]
    HighPotentialITT = 389040024,

    [Description("Primary and secondary undergraduate fee funded")]
    Primaryandsecondaryundergraduatefeefunded = 389040025,

    [Description("Primary and secondary postgraduate fee funded")]
    Primaryandsecondarypostgraduatefeefunded = 389040026
}

public static class IttProgrammeTypeExtensions
{
    public static dfeta_ITTProgrammeType ConvertToIttProgrammeType(this IttProgrammeType input) =>
        input.ConvertToEnumByValue<IttProgrammeType, dfeta_ITTProgrammeType>();

    public static bool TryConvertToIttProgrammeType(this IttProgrammeType input, out dfeta_ITTProgrammeType result) =>
        input.TryConvertToEnumByValue(out result);
}
