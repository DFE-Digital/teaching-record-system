using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.InductionExemptions;

public class InductionExemptionService(ReferenceDataCache referenceDataCache, TrsDbContext dbContext)
{
    private static Dictionary<ExemptionReasonCategory, IEnumerable<Guid>> ExemptionReasonCategoryMap => new()
    {
        { ExemptionReasonCategory.Miscellaneous, new List<Guid> {
            InductionExemptionReason.QualifiedThroughFurtherEducationRouteBetween1Sep2001And1Sep2004Id,
            InductionExemptionReason.ExemptDataLossOrErrorCriteriaId,
            InductionExemptionReason.QtlsId,
            InductionExemptionReason.OverseasTrainedTeacherId,
            InductionExemptionReason.ExemptId,
            InductionExemptionReason.QualifiedThroughEeaMutualRecognitionRouteId } },
        { ExemptionReasonCategory.HistoricalQualificationRoute, new List<Guid> {
            InductionExemptionReason.QualifiedBetween7May1999And1April2003FirstPostInWalesId,
            InductionExemptionReason.RegisteredTeacherWithAtLeast2YearsFullTimeTeachingExperienceId,
            InductionExemptionReason.QualifiedBefore7May2000Id } },
        { ExemptionReasonCategory.InductionCompletedOutsideEngland, new List<Guid> {
            InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId,
            InductionExemptionReason.PassedInductionInJerseyId,
            InductionExemptionReason.PassedInductionInNorthernIrelandId,
            InductionExemptionReason.PassedInWalesId,
            InductionExemptionReason.PassedInductionInServiceChildrensEducationSchoolsInGermanyOrCyprusId,
            InductionExemptionReason.PassedProbationaryPeriodInGibraltarId,
            InductionExemptionReason.PassedInductionInIsleOfManId,
            InductionExemptionReason.PassedInductionInGuernseyId } }
    };

    private static Dictionary<ExemptionReasonCategory, IEnumerable<Guid>> RoutesFeatureExemptionReasonCategoryMap => new()
    {
        { ExemptionReasonCategory.Miscellaneous, new List<Guid> {
            InductionExemptionReason.QualifiedThroughFurtherEducationRouteBetween1Sep2001And1Sep2004Id,
            InductionExemptionReason.ExemptDataLossOrErrorCriteriaId,
            InductionExemptionReason.ExemptId,
            InductionExemptionReason.QualifiedThroughEeaMutualRecognitionRouteId } },
        { ExemptionReasonCategory.HistoricalQualificationRoute, new List<Guid> {
            InductionExemptionReason.QualifiedBetween7May1999And1April2003FirstPostInWalesId,
            InductionExemptionReason.RegisteredTeacherWithAtLeast2YearsFullTimeTeachingExperienceId,
            InductionExemptionReason.QualifiedBefore7May2000Id } },
        { ExemptionReasonCategory.InductionCompletedOutsideEngland, new List<Guid> {
            InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId,
            InductionExemptionReason.PassedInductionInJerseyId,
            InductionExemptionReason.PassedInductionInNorthernIrelandId,
            InductionExemptionReason.PassedInWalesId,
            InductionExemptionReason.PassedInductionInServiceChildrensEducationSchoolsInGermanyOrCyprusId,
            InductionExemptionReason.PassedProbationaryPeriodInGibraltarId,
            InductionExemptionReason.PassedInductionInIsleOfManId,
            InductionExemptionReason.PassedInductionInGuernseyId } }
    };

    private static Dictionary<ExemptionReasonCategory, IEnumerable<InductionExemptionReason>> CreateFilteredDictionaryFromIds(InductionExemptionReason[] exemptionReasons)
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

    public async Task<ExemptionReasonsResponse> GetExemptionReasonsAsync(Guid personId)
    {
        var exemptionReasons = await referenceDataCache.GetPersonLevelInductionExemptionReasonsAsync(activeOnly: true);

        var RoutesWithInductionExemptions = dbContext.RouteToProfessionalStatuses
            .Include(p => p.RouteToProfessionalStatusType)
            .ThenInclude(r => r!.InductionExemptionReason)
            .Where(
                p => p.PersonId == personId &&
                p.ExemptFromInduction == true &&
                p.RouteToProfessionalStatusType!.InductionExemptionReason != null)
            .Select(r => new RouteWithExemption()
            {
                InductionExemptionReasonId = r.RouteToProfessionalStatusType!.InductionExemptionReasonId!.Value,
                RouteToProfessionalStatusId = r.RouteToProfessionalStatusTypeId,
                InductionExemptionReasonName = r.RouteToProfessionalStatusType.InductionExemptionReason!.Name,
                RouteToProfessionalStatusName = r.RouteToProfessionalStatusType.Name
            });

        // note: RoutesWithInductionExemptions is null if the Feature RoutesToProfessionalStatus isn't enabled
        if (RoutesWithInductionExemptions is not null && RoutesWithInductionExemptions.Any()) // exclude some exemptions from the choices if they apply because of a route
        {
            var exemptionReasonIdsToExclude = ExemptionsToBeExcludedIfRouteQualificationIsHeld
                .Join(RoutesWithInductionExemptions,
                    guid => guid,
                    r => r.InductionExemptionReasonId,
                    (guid, route) => route.InductionExemptionReasonId);

            var exemptionReasonsToInclude = RouteFeatureExemptionReasonIds
                .Where(id => !exemptionReasonIdsToExclude.Contains(id))
                .Join(exemptionReasons,
                    guid => guid,
                    exemption => exemption.InductionExemptionReasonId,
                    (guid, exemption) => exemption)
                .ToArray();

            return new()
            {
                RoutesWithInductionExemptions = RoutesWithInductionExemptions,
                ExemptionReasonCategories = CreateFilteredDictionaryFromIds(exemptionReasonsToInclude)
            };
        }
        else
        {
            var exemptionReasonsToInclude = ExemptionReasonIds
                .Join(exemptionReasons,
                        guid => guid,
                        exemption => exemption.InductionExemptionReasonId,
                        (guid, exemption) => exemption)
                    .ToArray();

            return new()
            {
                RoutesWithInductionExemptions = RoutesWithInductionExemptions,
                ExemptionReasonCategories = CreateFilteredDictionaryFromIds(exemptionReasonsToInclude)
            };
        }
    }
}

public enum ExemptionReasonCategory
{
    [Display(Name = "Miscellaneous exemptions")]
    Miscellaneous = 1,
    [Display(Name = "Exemptions for historical qualification routes")]
    HistoricalQualificationRoute = 2,
    [Display(Name = "Induction completed outside England")]
    InductionCompletedOutsideEngland = 3
}

public record ExemptionReasonsResponse
{
    public required IEnumerable<RouteWithExemption>? RoutesWithInductionExemptions { get; init; }
    public required Dictionary<ExemptionReasonCategory, IEnumerable<InductionExemptionReason>> ExemptionReasonCategories { get; init; }
}
