using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

// currently just uses a knowledge of page order combined with the FieldRequired method
// page will also need to know whether the route can have an exemption (if status is awarded/approved)
// and also need hasImplicitexemption - from InductionExemptionReason
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
            FieldRequirement pageRequired = currentPage switch
            {
                AddRoutePage.EndDate => QuestionDriverHelper.FieldRequired(route.TrainingEndDateRequired, status.GetEndDateRequirement()),
                AddRoutePage.AwardDate => QuestionDriverHelper.FieldRequired(route.AwardDateRequired, status.GetAwardDateRequirement()),
                AddRoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(route.InductionExemptionRequired, status.GetInductionExemptionRequirement()),
                AddRoutePage.Route => throw new NotImplementedException(),
                AddRoutePage.Status => throw new NotImplementedException(),
                AddRoutePage.StartDate => throw new NotImplementedException(),
                AddRoutePage.TrainingProvider => throw new NotImplementedException(),
                AddRoutePage.DegreeType => throw new NotImplementedException(),
                AddRoutePage.Country => throw new NotImplementedException(),
                AddRoutePage.AgeSpecialism => throw new NotImplementedException(),
                AddRoutePage.SubjectSpecialism => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (pageRequired != FieldRequirement.NotApplicable)
            { return page; }
        }
        return null;
    }

    public static AddRoutePage? BackPage(RouteToProfessionalStatus route, ProfessionalStatusStatus status, AddRoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .Where(p => p <= currentPage)
            .OrderByDescending(p => p);

        foreach (var page in pagesInOrder)
        {
            FieldRequirement pageRequired = currentPage switch
            {
                AddRoutePage.EndDate => QuestionDriverHelper.FieldRequired(route.TrainingEndDateRequired, status.GetEndDateRequirement()),
                AddRoutePage.AwardDate => QuestionDriverHelper.FieldRequired(route.AwardDateRequired, status.GetAwardDateRequirement()),
                AddRoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(route.InductionExemptionRequired, status.GetInductionExemptionRequirement()),
                AddRoutePage.Route => throw new NotImplementedException(),
                AddRoutePage.Status => throw new NotImplementedException(),
                AddRoutePage.StartDate => throw new NotImplementedException(),
                AddRoutePage.TrainingProvider => throw new NotImplementedException(),
                AddRoutePage.DegreeType => throw new NotImplementedException(),
                AddRoutePage.Country => throw new NotImplementedException(),
                AddRoutePage.AgeSpecialism => throw new NotImplementedException(),
                AddRoutePage.SubjectSpecialism => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (pageRequired != FieldRequirement.NotApplicable)
            { return page; }
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
