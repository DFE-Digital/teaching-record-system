using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public static class PageDriver
{
    public static AddRoutePage? NextPage(RouteToProfessionalStatus route, ProfessionalStatusStatus status, AddRoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .Where(p => p > currentPage)
            .OrderBy(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.ToFieldRequirement(route, status);

            if (pageRequired != FieldRequirement.NotApplicable)
            {
                return page;
            }
        }
        return null;
    }

    public static AddRoutePage? PreviousPage(RouteToProfessionalStatus route, ProfessionalStatusStatus status, AddRoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .Where(p => p < currentPage)
            .OrderByDescending(p => p);

        foreach (var page in pagesInOrder)
        {
            var pageRequired = page.ToFieldRequirement(route, status);

            if (pageRequired != FieldRequirement.NotApplicable)
            {
                return page;
            }
        }

        return null;
    }

    public static bool IsLastPage(AddRoutePage currentPage)
    {
        var lastPage = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .OrderByDescending(p => p)
            .First();
        return lastPage == currentPage;
    }
}

public static class QuestionDriverHelper
{
    public static FieldRequirement ToFieldRequirement(this AddRoutePage page, RouteToProfessionalStatus Route, ProfessionalStatusStatus Status)
    {
        return page switch
        {
            AddRoutePage.EndDate => QuestionDriverHelper.FieldRequired(Route.TrainingEndDateRequired, Status.GetEndDateRequirement()),
            AddRoutePage.AwardDate => QuestionDriverHelper.FieldRequired(Route.AwardDateRequired, Status.GetAwardDateRequirement()),
            AddRoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(Route.InductionExemptionRequired, Status.GetInductionExemptionRequirement()),
            AddRoutePage.Route => FieldRequirement.Mandatory,
            AddRoutePage.Status => FieldRequirement.Mandatory,
            AddRoutePage.StartDate => QuestionDriverHelper.FieldRequired(Route.TrainingStartDateRequired, Status.GetStartDateRequirement()),
            AddRoutePage.TrainingProvider => QuestionDriverHelper.FieldRequired(Route.TrainingProviderRequired, Status.GetTrainingProviderRequirement()),
            AddRoutePage.DegreeType => QuestionDriverHelper.FieldRequired(Route.DegreeTypeRequired, Status.GetDegreeTypeRequirement()),
            AddRoutePage.Country => QuestionDriverHelper.FieldRequired(Route.TrainingCountryRequired, Status.GetCountryRequirement()),
            AddRoutePage.AgeSpecialism => QuestionDriverHelper.FieldRequired(Route.TrainingAgeSpecialismTypeRequired, Status.GetAgeSpecialismRequirement()),
            AddRoutePage.SubjectSpecialism => QuestionDriverHelper.FieldRequired(Route.TrainingSubjectsRequired, Status.GetSubjectsRequirement()),
            AddRoutePage.CheckYourAnswers => FieldRequirement.Mandatory,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }

    public static FieldRequirement FieldRequired(FieldRequirement routeFieldRequirement, FieldRequirement statusFieldRequirement)
    {
        if (routeFieldRequirement == FieldRequirement.NotApplicable || statusFieldRequirement == FieldRequirement.NotApplicable)
        {
            return FieldRequirement.NotApplicable;
        }
        else if (routeFieldRequirement == FieldRequirement.Mandatory || statusFieldRequirement == FieldRequirement.Mandatory)
        {
            return FieldRequirement.Mandatory;
        }
        else
        {
            return FieldRequirement.Optional;
        }
    }
}
