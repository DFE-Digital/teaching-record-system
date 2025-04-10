using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public class AddRouteCommonPageModel : PageModel
{
    public AddRouteCommonPageModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    {
        _linkGenerator = linkGenerator;
        _referenceDataCache = referenceDataCache;
    }

    protected TrsLinkGenerator _linkGenerator;
    protected ReferenceDataCache _referenceDataCache;

    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public RouteToProfessionalStatus? Route { get; set; }
    public ProfessionalStatusStatus Status { get; set; }

    // currently just uses a knowledge of page order combined with the FieldRequired method
    // page will also need to know whether the route can have an exemption (if status is awarded/approved)
    // and also need hasImplicitexemption - from InductionExemptionReason
    public AddRoutePage? NextPage(AddRoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .Where(p => p > currentPage)
            .OrderBy(p => p);

        foreach (var page in pagesInOrder)
        {
            FieldRequirement pageRequired = currentPage switch
            {
                AddRoutePage.EndDate => QuestionDriverHelper.FieldRequired(Route!.TrainingEndDateRequired, Status.GetEndDateRequirement()),
                AddRoutePage.AwardDate => QuestionDriverHelper.FieldRequired(Route!.AwardDateRequired, Status.GetAwardDateRequirement()),
                AddRoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(Route!.InductionExemptionRequired, Status.GetInductionExemptionRequirement()),
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
    public AddRoutePage? PreviousPage(AddRoutePage currentPage)
    {
        var pagesInOrder = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .Where(p => p <= currentPage)
            .OrderByDescending(p => p);

        foreach (var page in pagesInOrder)
        {
            FieldRequirement pageRequired = page switch
            {
                AddRoutePage.EndDate => QuestionDriverHelper.FieldRequired(Route!.TrainingEndDateRequired, Status.GetEndDateRequirement()),
                AddRoutePage.AwardDate => QuestionDriverHelper.FieldRequired(Route!.AwardDateRequired, Status.GetAwardDateRequirement()),
                AddRoutePage.InductionExemption => QuestionDriverHelper.FieldRequired(Route!.InductionExemptionRequired, Status.GetInductionExemptionRequirement()),
                AddRoutePage.Route => FieldRequirement.Mandatory,
                AddRoutePage.Status => FieldRequirement.Mandatory,
                AddRoutePage.StartDate => QuestionDriverHelper.FieldRequired(Route!.TrainingStartDateRequired, Status.GetStartDateRequirement()),
                AddRoutePage.TrainingProvider => throw new NotImplementedException(),
                AddRoutePage.DegreeType => throw new NotImplementedException(),
                AddRoutePage.Country => throw new NotImplementedException(),
                AddRoutePage.AgeSpecialism => throw new NotImplementedException(),
                AddRoutePage.SubjectSpecialism => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(page))
            };

            if (pageRequired != FieldRequirement.NotApplicable)
            {
                return page;
            }
        }
        return null;
    }

    public bool IsLastPage(AddRoutePage currentPage)
    {
        var lastPage = Enum.GetValues(typeof(AddRoutePage))
            .Cast<AddRoutePage>()
            .OrderByDescending(p => p)
            .First();
        return lastPage == currentPage;
    }

    public string PageAddress(AddRoutePage page)
    {
        return page switch
        {
            AddRoutePage.EndDate => _linkGenerator.RouteAddEndDate(PersonId, JourneyInstance!.InstanceId),
            AddRoutePage.StartDate => _linkGenerator.RouteAddStartDate(PersonId, JourneyInstance!.InstanceId),
            AddRoutePage.Status => _linkGenerator.RouteAddStatus(PersonId, JourneyInstance!.InstanceId),
            AddRoutePage.Route => _linkGenerator.RouteAddRoute(PersonId, JourneyInstance!.InstanceId),
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!(JourneyInstance!.State.RouteToProfessionalStatusId.HasValue && JourneyInstance!.State.Status.HasValue))
        {
            context.Result = new BadRequestResult();
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        Route = await _referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId!.Value);
        Status = JourneyInstance!.State.Status!.Value;
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
