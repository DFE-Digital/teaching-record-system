using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public static class PageDriver
{
    public static AddRoutePage? NextPage(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status, AddRoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .Where(p => p > currentPage)
            .OrderBy(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.FieldRequirementForPage(route, status);

            if (pageRequired != FieldRequirement.NotApplicable)
            {
                // if the route has an implicit exemption, don't show the induction exemption page
                if (page == AddRoutePage.InductionExemption
                    && route.InductionExemptionReason is not null
                    && route.InductionExemptionReason.RouteImplicitExemption)
                {
                    continue;
                }
                else
                {
                    return page;
                }
            }
        }

        return null;
    }

    public static AddRoutePage? PreviousPage(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status, AddRoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .Where(p => p < currentPage)
            .OrderByDescending(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.FieldRequirementForPage(route, status);

            if (pageRequired != FieldRequirement.NotApplicable)
            {
                // if the route has an implicit exemption, don't show the induction exemption page
                if (page == AddRoutePage.InductionExemption
                    && route.InductionExemptionReason is not null
                    && route.InductionExemptionReason.RouteImplicitExemption)
                {
                    continue;
                }
                else
                {
                    return page;
                }
            }
        }

        return null;
    }

    public static FieldRequirement FieldRequirementForPage(this AddRoutePage page, RouteToProfessionalStatusType Route, RouteToProfessionalStatusStatus Status)
    {
        return page switch
        {
            AddRoutePage.StartAndEndDate => QuestionDriverHelper.FieldRequired(Route.TrainingEndDateRequired, Status.GetEndDateRequirement()),
            AddRoutePage.HoldsFrom => QuestionDriverHelper.FieldRequired(Route.HoldsFromRequired, Status.GetAwardDateRequirement()),
            AddRoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(Route.InductionExemptionRequired, Status.GetInductionExemptionRequirement()),
            AddRoutePage.Route => FieldRequirement.Mandatory,
            AddRoutePage.Status => FieldRequirement.Mandatory,
            AddRoutePage.TrainingProvider => QuestionDriverHelper.FieldRequired(Route.TrainingProviderRequired, Status.GetTrainingProviderRequirement()),
            AddRoutePage.DegreeType => QuestionDriverHelper.FieldRequired(Route.DegreeTypeRequired, Status.GetDegreeTypeRequirement()),
            AddRoutePage.Country => QuestionDriverHelper.FieldRequired(Route.TrainingCountryRequired, Status.GetCountryRequirement()),
            AddRoutePage.AgeRangeSpecialism => QuestionDriverHelper.FieldRequired(Route.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement()),
            AddRoutePage.SubjectSpecialisms => QuestionDriverHelper.FieldRequired(Route.TrainingSubjectsRequired, Status.GetSubjectsRequirement()),
            AddRoutePage.ChangeReason => FieldRequirement.Mandatory,
            AddRoutePage.CheckYourAnswers => FieldRequirement.Mandatory,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }
}
