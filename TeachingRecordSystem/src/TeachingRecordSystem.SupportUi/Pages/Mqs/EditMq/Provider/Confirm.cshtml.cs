using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

[Journey(JourneyNames.EditMqProvider), RequireJourneyInstance]
public class ConfirmModel(
    TrsDbContext dbContext,
    IClock clock,
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditMqProviderState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? CurrentProviderName { get; set; }

    public Guid NewProviderId { get; set; }

    public string? NewProviderName { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.QualificationId == QualificationId);
        qualification.ProviderId = NewProviderId;
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

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqEditProvider(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        var newAndOldProviders = await dbContext.MandatoryQualificationProviders
            .Where(p =>
                p.MandatoryQualificationProviderId == JourneyInstance!.State.CurrentProviderId ||
                p.MandatoryQualificationProviderId == JourneyInstance!.State.ProviderId)
            .ToDictionaryAsync(p => p.MandatoryQualificationProviderId, p => p);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentProviderName = JourneyInstance!.State.CurrentProviderId is Guid currentProviderId ? newAndOldProviders[currentProviderId].Name : null;
        NewProviderId = JourneyInstance!.State.ProviderId.Value;
        NewProviderName = newAndOldProviders[NewProviderId].Name;

        await next();
    }
}
