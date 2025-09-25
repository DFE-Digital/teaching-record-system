using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class ConfirmKeepRecordSeparateReasonModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator, IClock clock) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
{
    public string? Reason { get; set; }
    public KeepingRecordSeparateReason? KeepSeparateReason { get; set; }

    public void OnGet()
    {
        Reason = JourneyInstance!.State.Reason;
        KeepSeparateReason = JourneyInstance!.State.KeepSeparateReason;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var person = await DbContext.Persons.SingleAsync(x => x.PersonId == supportTask.PersonId);
        var requestData = supportTask.TrnRequestMetadata!;
        var state = JourneyInstance!.State;
        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);
        var now = clock.UtcNow;
        requestData.SetResolvedPerson(supportTask.PersonId!.Value!);

        // Conditionally override the value in Reason.
        // if KeepSeparateReason is AnotherReason - the event will contain the reason provided from the user
        // if RecordDoesNotMatch, override reason using the display name from the enum
        if (JourneyInstance!.State.KeepSeparateReason == KeepingRecordSeparateReason.RecordDoesNotMatch)
        {
            Reason = JourneyInstance!.State.KeepSeparateReason.GetDisplayName()!;
        }
        else if (JourneyInstance!.State.KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason)
        {
            Reason = JourneyInstance!.State.Reason;
        }

        supportTask.UpdateData<TeacherPensionsPotentialDuplicateData>(data => data with
        {
            ResolvedAttributes = null,
            SelectedPersonAttributes = null //do we need to set the person attributes
        });

        //create event
        var @event = new TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent()
        {
            PersonId = supportTask.PersonId.Value!,
            RequestData = EventModels.TrnRequestMetadata.FromModel(requestData),
            ChangeReason = TeacherPensionsPotentialDuplicateSupportTaskResolvedReason.RecordKept,
            Changes = TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.None,
            PersonAttributes = EventModels.PersonAttributes.FromModel(person!),
            OldPersonAttributes = EventModels.PersonAttributes.FromModel(person!),
            SupportTask = EventModels.SupportTask.FromModel(supportTask),
            OldSupportTask = oldSupportTaskEventModel,
            Comments = Reason,
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = User.GetUserId()
        };

        await DbContext.AddEventAndBroadcastAsync(@event);
        await DbContext.SaveChangesAsync();

        supportTask.Status = SupportTaskStatus.Closed;
        supportTask.UpdatedOn = now;
        await DbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            "Teachers’ Pensions duplicate task completed",
            $"The records were not merged.");

        await JourneyInstance!.CompleteAsync();
        return Redirect(linkGenerator.TeacherPensions());
    }

    public string GetReason()
    {
        if (KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason)
        {
            return Reason!;
        }
        else if (KeepSeparateReason == KeepingRecordSeparateReason.RecordDoesNotMatch)
        {
            return KeepSeparateReason.Value.GetDisplayName()!;
        }
        return string.Empty;
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.TeacherPensions());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance!.State.KeepSeparateReason is null ||
            JourneyInstance!.State.KeepSeparateReason == KeepingRecordSeparateReason.AnotherReason && string.IsNullOrEmpty(JourneyInstance!.State.Reason))
        {
            context.Result = Redirect(linkGenerator.TeacherPensions());
            return;
        }
        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
