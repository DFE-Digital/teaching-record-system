using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

[Journey(JourneyNames.EditMqResult), RequireJourneyInstance]
public class ConfirmModel(
    TrsDbContext dbContext,
    IClock clock,
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditMqResultState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public MandatoryQualificationStatus? CurrentStatus { get; set; }

    public MandatoryQualificationStatus? NewStatus { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? NewEndDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.QualificationId == QualificationId);
        qualification.Status = NewStatus;
        qualification.EndDate = NewEndDate;
        qualification.UpdatedOn = clock.UtcNow;

        // TODO Audit event

        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification changed");

        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqEditStatus(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentStatus = JourneyInstance!.State.CurrentStatus;
        NewStatus ??= JourneyInstance!.State.Status;
        CurrentEndDate = JourneyInstance!.State.CurrentEndDate;
        NewEndDate ??= JourneyInstance!.State.EndDate;
    }
}
