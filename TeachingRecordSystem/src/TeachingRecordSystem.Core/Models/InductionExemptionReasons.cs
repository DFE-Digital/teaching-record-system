namespace TeachingRecordSystem.Core.Models;

[Flags]
public enum InductionExemptionReasons
{
    None = 0,
    QualifiedBefore07052000 = 1 << 0,
    QualifiedBetween07051999And01042003FirstPostWasInWalesAndLastedAMinimumOfTwoTerms = 1 << 1,
    QualifiedThroughFurtherEducationRouteBetween01092001And01092004 = 1 << 2,
    PassedInductionInGuernsey = 1 << 3,
    PassedInductionInIsleOfMan = 1 << 4,
    PassedInductionInJersey = 1 << 5,
    PassedInductionInNorthernIreland = 1 << 6,
    PassedInductionInServiceChildrensEducationSchoolsInGermanyOrCyprus = 1 << 7,
    PassedInductionInWales = 1 << 8,
    PassedProbationaryPeriodInGibraltar = 1 << 9,
    Exempt = 1 << 10,
    ExemptDataLossOrErrorCriteria = 1 << 11,
    HasOrIsEligibleForFullRegistrationInScotland = 1 << 12,
    OverseasTrainedTeacher = 1 << 13,
    QualifiedThroughEeaMutualRecognitionRoute = 1 << 14,
    RegisteredTeacherWithAtLeast2YearsFullTimeTeachingExperience = 1 << 15,
    ExemptThroughQtlsProvidedTheyMaintainMembershipOfTheSocietyOfEducationAndTraining = 1 << 16,
}
