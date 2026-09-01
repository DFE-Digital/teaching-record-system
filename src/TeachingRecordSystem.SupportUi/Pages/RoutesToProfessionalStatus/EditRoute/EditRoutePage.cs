using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

// The questions this journey can ask, in the order the detail page lists them. The route type itself
// can't be changed, so unlike adding a route there's no question for it.
public enum EditRoutePage
{
    Status = 0,
    StartAndEndDate,
    HoldsFrom,
    InductionExemption,
    TrainingProvider,
    DegreeType,
    Country,
    AgeRangeSpecialism,
    SubjectSpecialisms,
    ChangeReason,
    CheckAnswers
}

public static class EditRoutePageExtensions
{
    extension(EditRoutePage page)
    {
        public FieldRequirement GetFieldRequirement(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status)
        {
            return page switch
            {
                EditRoutePage.Status => FieldRequirement.Mandatory,
                EditRoutePage.StartAndEndDate => QuestionDriverHelper.FieldRequired(route.TrainingEndDateRequired, status.GetEndDateRequirement()),
                EditRoutePage.HoldsFrom => QuestionDriverHelper.FieldRequired(route.HoldsFromRequired, status.GetHoldsFromDateRequirement()),
                EditRoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(route.InductionExemptionRequired, status.GetInductionExemptionRequirement()),
                EditRoutePage.TrainingProvider => QuestionDriverHelper.FieldRequired(route.TrainingProviderRequired, status.GetTrainingProviderRequirement()),
                EditRoutePage.DegreeType => QuestionDriverHelper.FieldRequired(route.DegreeTypeRequired, status.GetDegreeTypeRequirement()),
                EditRoutePage.Country => QuestionDriverHelper.FieldRequired(route.TrainingCountryRequired, status.GetCountryRequirement()),
                EditRoutePage.AgeRangeSpecialism => QuestionDriverHelper.FieldRequired(route.TrainingAgeSpecialismTypeRequired, status.GetAgeSpecialismRequirement()),
                EditRoutePage.SubjectSpecialisms => QuestionDriverHelper.FieldRequired(route.TrainingSubjectsRequired, status.GetSubjectsRequirement()),
                EditRoutePage.ChangeReason => FieldRequirement.Mandatory,
                EditRoutePage.CheckAnswers => FieldRequirement.Mandatory,
                _ => throw new ArgumentOutOfRangeException(nameof(page))
            };
        }

        public bool AppliesToRoute(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status) =>
            page switch
            {
                EditRoutePage.InductionExemption when route.InductionExemptionReason?.RouteImplicitExemption == true => false,
                _ => page.GetFieldRequirement(route, status) != FieldRequirement.NotApplicable
            };
    }

    // A route that comes with an induction exemption of its own doesn't ask whether it provides one.
}
