using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class CheckAnswersModel(
    TrsDbContext dbContext,
    IClock clock,
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public Guid ProviderId { get; set; }

    public string? ProviderName { get; set; }

    public MandatoryQualificationSpecialism Specialism { get; set; }

    public DateOnly StartDate { get; set; }

    public MandatoryQualificationStatus Status { get; set; }

    public DateOnly? EndDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        dbContext.MandatoryQualifications.Add(new()
        {
            QualificationId = Guid.NewGuid(),
            CreatedOn = clock.UtcNow,
            UpdatedOn = clock.UtcNow,
            PersonId = PersonId,
            ProviderId = ProviderId,
            Status = Status,
            Specialism = Specialism,
            StartDate = StartDate,
            EndDate = EndDate
        });

        // TODO Add audit event

        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification added");

        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonDetail(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqAddProvider(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        ProviderId = JourneyInstance.State.ProviderId.Value;
        ProviderName = (await dbContext.MandatoryQualificationProviders.SingleAsync(p => p.MandatoryQualificationProviderId == ProviderId)).Name;
        Specialism = JourneyInstance.State.Specialism.Value;
        StartDate = JourneyInstance.State.StartDate.Value;
        Status = JourneyInstance.State.Status.Value;
        EndDate = JourneyInstance.State.EndDate;

        await next();
    }
}
