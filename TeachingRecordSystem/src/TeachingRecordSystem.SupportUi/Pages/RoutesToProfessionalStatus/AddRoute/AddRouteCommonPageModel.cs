using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public abstract class AddRouteCommonPageModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : PageModel
{
    protected TrsLinkGenerator LinkGenerator => linkGenerator;

    protected ReferenceDataCache ReferenceDataCache => referenceDataCache;

    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public RouteToProfessionalStatusType Route { get; set; } = null!;
    public RouteToProfessionalStatusStatus Status { get; set; }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(LinkGenerator.PersonQualifications(PersonId));
    }

    protected abstract RoutePage CurrentPage { get; }

    protected string NextPageUrl =>
        NextPage is RoutePage nextPage && nextPage == CurrentPage
            ? LinkGenerator.PersonQualifications(PersonId)
            : LinkGenerator.RouteAddPage(nextPage, PersonId, JourneyInstance!.InstanceId);

    protected string PreviousPageUrl =>
        PreviousPage is RoutePage previousPage && previousPage == CurrentPage
            ? LinkGenerator.PersonQualifications(PersonId)
            : LinkGenerator.RouteAddPage(previousPage, PersonId, JourneyInstance!.InstanceId);

    protected RoutePage NextPage =>
        PageDriver.NextPage(Route, Status, CurrentPage, FromCheckAnswers, JourneyInstance!.State);

    protected RoutePage PreviousPage =>
        PageDriver.PreviousPage(Route, Status, CurrentPage, FromCheckAnswers, JourneyInstance!.State);

    //public bool IsLastPage(AddRoutePage currentPage)
    //{
    //    return PageDriver.IsLastPage(currentPage);
    //}

    protected async Task<IActionResult> ContinueAsync()
    {
        if (NextPage == RoutePage.CheckYourAnswers && JourneyInstance!.State.NewRouteToProfessionalStatusId != null)
        {
            await JourneyInstance!.UpdateStateAsync(state =>
            {
                state.RouteToProfessionalStatusId = state.NewRouteToProfessionalStatusId;
                state.Status = state.NewStatus;
                state.HoldsFrom = state.NewHoldsFrom;
                state.TrainingStartDate = state.NewTrainingStartDate;
                state.TrainingEndDate = state.NewTrainingEndDate;
                state.TrainingSubjectIds = state.NewTrainingSubjectIds;
                state.TrainingAgeSpecialismType = state.NewTrainingAgeSpecialismType;
                state.TrainingAgeSpecialismRangeFrom = state.NewTrainingAgeSpecialismRangeFrom;
                state.TrainingAgeSpecialismRangeTo = state.NewTrainingAgeSpecialismRangeTo;
                state.TrainingCountryId = state.NewTrainingCountryId;
                state.TrainingProviderId = state.NewTrainingProviderId;
                state.IsExemptFromInduction = state.NewIsExemptFromInduction;
                state.DegreeTypeId = state.NewDegreeTypeId;
                state.ChangeReason = state.NewChangeReason;
                state.ChangeReasonDetail.HasAdditionalReasonDetail = state.NewChangeReasonDetail.HasAdditionalReasonDetail;
                state.ChangeReasonDetail.ChangeReasonDetail = state.NewChangeReasonDetail.ChangeReasonDetail;
                state.ChangeReasonDetail.HasAdditionalReasonDetail = state.NewChangeReasonDetail.HasAdditionalReasonDetail;
                state.ChangeReasonDetail.UploadEvidence = state.NewChangeReasonDetail.UploadEvidence;
                state.ChangeReasonDetail.EvidenceFileId = state.NewChangeReasonDetail.EvidenceFileId;
                state.ChangeReasonDetail.EvidenceFileName = state.NewChangeReasonDetail.EvidenceFileName;
                state.ChangeReasonDetail.EvidenceFileSizeDescription = state.NewChangeReasonDetail.EvidenceFileSizeDescription;

                state.NewRouteToProfessionalStatusId = null;
                state.NewStatus = null;
                state.NewHoldsFrom = null;
                state.NewTrainingStartDate = null;
                state.NewTrainingEndDate = null;
                state.NewTrainingSubjectIds = [];
                state.NewTrainingAgeSpecialismType = null;
                state.NewTrainingAgeSpecialismRangeFrom = null;
                state.NewTrainingAgeSpecialismRangeTo = null;
                state.NewTrainingCountryId = null;
                state.NewTrainingProviderId = null;
                state.NewIsExemptFromInduction = null;
                state.NewDegreeTypeId = null;
                state.NewChangeReason = null;
                state.NewChangeReasonDetail.HasAdditionalReasonDetail = null;
                state.NewChangeReasonDetail.ChangeReasonDetail = null;
                state.NewChangeReasonDetail.HasAdditionalReasonDetail = null;
                state.NewChangeReasonDetail.UploadEvidence = null;
                state.NewChangeReasonDetail.EvidenceFileId = null;
                state.NewChangeReasonDetail.EvidenceFileName = null;
                state.NewChangeReasonDetail.EvidenceFileSizeDescription = null;
            });
        }

        return Redirect(NextPageUrl);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (CurrentPage > RoutePage.Route && !JourneyInstance!.State.RouteToProfessionalStatusId.HasValue ||
            CurrentPage > RoutePage.Status && !JourneyInstance!.State.Status.HasValue)
        {
            context.Result = new BadRequestResult();
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        if (JourneyInstance!.State.RouteToProfessionalStatusId.HasValue)
        {
            Route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
        }

        if (JourneyInstance!.State.Status.HasValue)
        {
            Status = JourneyInstance!.State.Status.Value;
        }

        if (CurrentPage != RoutePage.CheckYourAnswers && JourneyInstance!.State.NewRouteToProfessionalStatusId == null)
        {
            await JourneyInstance!.UpdateStateAsync(state =>
            {
                state.NewRouteToProfessionalStatusId = state.RouteToProfessionalStatusId;
                state.NewStatus = state.Status;
                state.NewHoldsFrom = state.HoldsFrom;
                state.NewTrainingStartDate = state.TrainingStartDate;
                state.NewTrainingEndDate = state.TrainingEndDate;
                state.NewTrainingSubjectIds = state.TrainingSubjectIds;
                state.NewTrainingAgeSpecialismType = state.TrainingAgeSpecialismType;
                state.NewTrainingAgeSpecialismRangeFrom = state.TrainingAgeSpecialismRangeFrom;
                state.NewTrainingAgeSpecialismRangeTo = state.TrainingAgeSpecialismRangeTo;
                state.NewTrainingCountryId = state.TrainingCountryId;
                state.NewTrainingProviderId = state.TrainingProviderId;
                state.NewIsExemptFromInduction = state.IsExemptFromInduction;
                state.NewDegreeTypeId = state.DegreeTypeId;
                state.NewChangeReason = state.ChangeReason;
                state.NewChangeReasonDetail.HasAdditionalReasonDetail = state.ChangeReasonDetail.HasAdditionalReasonDetail;
                state.NewChangeReasonDetail.ChangeReasonDetail = state.ChangeReasonDetail.ChangeReasonDetail;
                state.NewChangeReasonDetail.HasAdditionalReasonDetail = state.ChangeReasonDetail.HasAdditionalReasonDetail;
                state.NewChangeReasonDetail.UploadEvidence = state.ChangeReasonDetail.UploadEvidence;
                state.NewChangeReasonDetail.EvidenceFileId = state.ChangeReasonDetail.EvidenceFileId;
                state.NewChangeReasonDetail.EvidenceFileName = state.ChangeReasonDetail.EvidenceFileName;
                state.NewChangeReasonDetail.EvidenceFileSizeDescription = state.ChangeReasonDetail.EvidenceFileSizeDescription;
            });
        }

        OnPageHandlerExecuting(context);
        await OnPageHandlerExecutingAsync(context);
        if (context.Result == null)
        {
            var executedContext = await next();
            OnPageHandlerExecuted(executedContext);
            await OnPageHandlerExecutedAsync(executedContext);
        }
    }

    protected virtual Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
        => Task.CompletedTask;

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;

}
