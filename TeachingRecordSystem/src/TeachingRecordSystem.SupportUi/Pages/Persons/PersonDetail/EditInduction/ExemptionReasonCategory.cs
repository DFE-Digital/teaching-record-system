using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public enum ExemptionReasonCategory
{
    [Display(Name = "Miscellaneous exemptions")]
    Miscellaneous = 1,
    [Display(Name = "Exemptions for historical qualification routes")]
    HistoricalQualificationRoute = 2,
    [Display(Name = "Induction completed outside England")]
    InductionCompletedOutsideEngland = 3
}

public static class ExemptionReasonCategories
{
    private static Dictionary<ExemptionReasonCategory, IEnumerable<Guid>> ExemptionReasonCategoryMap => new()
    {
        { ExemptionReasonCategory.Miscellaneous, new List<Guid> {
            InductionExemptionReason.QualifiedThroughFurtherEducationRouteBetween1Sep2001And1Sep2004Id,
            InductionExemptionReason.ExemptDataLossOrErrorCriteriaId,
            InductionExemptionReason.QtlsId,
            InductionExemptionReason.OverseasTrainedTeacherId,
            InductionExemptionReason.ExemptId,
            InductionExemptionReason.QualifiedThroughEEAMutualRecognitionRouteId} },
        { ExemptionReasonCategory.HistoricalQualificationRoute, new List<Guid> {
            InductionExemptionReason.QualifiedBetween7May1999And1April2003FirstPostInWalesId,
            InductionExemptionReason.RegisteredTeacherWithAtLeast2YearsFullTimeTeachingExperienceId,
            InductionExemptionReason.QualifiedBefore7May2000Id} },
        { ExemptionReasonCategory.InductionCompletedOutsideEngland, new List<Guid> {
            InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId,
            InductionExemptionReason.PassedInductionInJerseyId,
            InductionExemptionReason.PassedInductionInNorthernIrelandId,
            InductionExemptionReason.PassedInWalesId,
            InductionExemptionReason.PassedInductionInServiceChildrensEducationSchoolsInGermanyOrCyprusId,
            InductionExemptionReason.PassedProbationaryPeriodInGibraltarId,
            InductionExemptionReason.PassedInductionInIsleOfManId,
            InductionExemptionReason.PassedInductionInGuernseyId} }
    };

    private static Dictionary<ExemptionReasonCategory, IEnumerable<Guid>> RoutesFeatureExemptionReasonCategoryMap => new()
    {
        { ExemptionReasonCategory.Miscellaneous, new List<Guid> {
            InductionExemptionReason.QualifiedThroughFurtherEducationRouteBetween1Sep2001And1Sep2004Id,
            InductionExemptionReason.ExemptDataLossOrErrorCriteriaId,
            InductionExemptionReason.ExemptId,
            InductionExemptionReason.QualifiedThroughEEAMutualRecognitionRouteId} },
        { ExemptionReasonCategory.HistoricalQualificationRoute, new List<Guid> {
            InductionExemptionReason.QualifiedBetween7May1999And1April2003FirstPostInWalesId,
            InductionExemptionReason.RegisteredTeacherWithAtLeast2YearsFullTimeTeachingExperienceId,
            InductionExemptionReason.QualifiedBefore7May2000Id} },
        { ExemptionReasonCategory.InductionCompletedOutsideEngland, new List<Guid> {
            InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId,
            InductionExemptionReason.PassedInductionInJerseyId,
            InductionExemptionReason.PassedInductionInNorthernIrelandId,
            InductionExemptionReason.PassedInWalesId,
            InductionExemptionReason.PassedInductionInServiceChildrensEducationSchoolsInGermanyOrCyprusId,
            InductionExemptionReason.PassedProbationaryPeriodInGibraltarId,
            InductionExemptionReason.PassedInductionInIsleOfManId,
            InductionExemptionReason.PassedInductionInGuernseyId} }
    };

    public static Dictionary<ExemptionReasonCategory, IEnumerable<InductionExemptionReason>> CreateFilteredDictionaryFromIds(InductionExemptionReason[] exemptionReasons)
    {
        return ExemptionReasonCategoryMap
            .ToDictionary(
                kvp => kvp.Key,
                kvp => exemptionReasons.Where(r => kvp.Value.Contains(r.InductionExemptionReasonId))
            )
            .Where(kvp => kvp.Value.Any())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }


    public static IEnumerable<Guid> ExemptionsToBeExcludedIfRouteQualificationIsHeld =>
        new List<Guid>() {
            InductionExemptionReason.PassedInductionInNorthernIrelandId,
            InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId
        };

    public static IEnumerable<Guid> ExemptionReasonIds =>
        ExemptionReasonCategoryMap.Values.SelectMany(g => g);

    public static IEnumerable<Guid> RouteFeatureExemptionReasonIds =>
        RoutesFeatureExemptionReasonCategoryMap.Values.SelectMany(g => g);
}
