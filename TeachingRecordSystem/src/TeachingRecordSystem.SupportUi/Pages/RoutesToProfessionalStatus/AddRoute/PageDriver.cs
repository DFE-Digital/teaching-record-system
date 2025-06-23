using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public static class PageDriver
{
    public static RoutePage? NextPage(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status, RoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(RoutePage))
            .Cast<RoutePage>()
            .Where(p => p > currentPage)
            .OrderBy(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.FieldRequirementForPage(route, status);

            if (pageRequired != FieldRequirement.NotApplicable)
            {
                // if the route has an implicit exemption, don't show the induction exemption page
                if (page == RoutePage.InductionExemption
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

    public static RoutePage? PreviousPage(RouteToProfessionalStatusType route, RouteToProfessionalStatusStatus status, RoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(RoutePage))
            .Cast<RoutePage>()
            .Where(p => p < currentPage)
            .OrderByDescending(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.FieldRequirementForPage(route, status);

            if (pageRequired != FieldRequirement.NotApplicable)
            {
                // if the route has an implicit exemption, don't show the induction exemption page
                if (page == RoutePage.InductionExemption
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

    //public static bool IsLastPage(AddRoutePage currentPage)
    //{
    //    var lastPage = Enum.GetValues(typeof(AddRoutePage))
    //        .Cast<AddRoutePage>()
    //        .OrderByDescending(p => p)
    //        .First();

    //    return lastPage == currentPage;
    //}

    public static FieldRequirement FieldRequirementForPage(this RoutePage page, RouteToProfessionalStatusType Route, RouteToProfessionalStatusStatus Status)
    {
        return page switch
        {
            RoutePage.StartAndEndDate => QuestionDriverHelper.FieldRequired(Route.TrainingEndDateRequired, Status.GetEndDateRequirement()),
            RoutePage.HoldsFrom => QuestionDriverHelper.FieldRequired(Route.HoldsFromRequired, Status.GetAwardDateRequirement()),
            RoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(Route.InductionExemptionRequired, Status.GetInductionExemptionRequirement()),
            RoutePage.Route => FieldRequirement.Mandatory,
            RoutePage.Status => FieldRequirement.Mandatory,
            RoutePage.TrainingProvider => QuestionDriverHelper.FieldRequired(Route.TrainingProviderRequired, Status.GetTrainingProviderRequirement()),
            RoutePage.DegreeType => QuestionDriverHelper.FieldRequired(Route.DegreeTypeRequired, Status.GetDegreeTypeRequirement()),
            RoutePage.Country => QuestionDriverHelper.FieldRequired(Route.TrainingCountryRequired, Status.GetCountryRequirement()),
            RoutePage.AgeRangeSpecialism => QuestionDriverHelper.FieldRequired(Route.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement()),
            RoutePage.SubjectSpecialisms => QuestionDriverHelper.FieldRequired(Route.TrainingSubjectsRequired, Status.GetSubjectsRequirement()),
            RoutePage.ChangeReason => FieldRequirement.Mandatory,
            RoutePage.CheckYourAnswers => FieldRequirement.Mandatory,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }
}
