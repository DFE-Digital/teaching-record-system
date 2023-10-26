using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class ConfirmModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ReferenceDataCache _referenceDataCache;

    public ConfirmModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
        _referenceDataCache = referenceDataCache;
    }

    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public Guid? AlertTypeId { get; set; }

    public string? AlertType { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public DateOnly? StartDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await _crmQueryDispatcher.ExecuteQuery(
            new CreateSanctionQuery()
            {
                ContactId = PersonId,
                SanctionCodeId = AlertTypeId!.Value,
                Details = Details!,
                Link = Link,
                StartDate = StartDate!.Value
            });

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Alert added");

        return Redirect(_linkGenerator.PersonAlerts(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var person = await _crmQueryDispatcher.ExecuteQuery(new GetContactDetailByIdQuery(PersonId, new ColumnSet(Contact.Fields.Id)));
        if (person is null)
        {
            context.Result = NotFound();
            return;
        }

        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(_linkGenerator.AlertAdd(PersonId, JourneyInstance!.InstanceId));
            return;
        }

        var sanctionCodeId = JourneyInstance!.State.AlertTypeId!.Value;
        var sanctionCode = await _referenceDataCache.GetSanctionCodeById(sanctionCodeId);

        AlertTypeId = sanctionCodeId;
        AlertType = sanctionCode.dfeta_name;
        Details = JourneyInstance!.State.Details;
        Link = JourneyInstance!.State.Link;
        StartDate = JourneyInstance!.State.StartDate;

        await next();
    }
}
