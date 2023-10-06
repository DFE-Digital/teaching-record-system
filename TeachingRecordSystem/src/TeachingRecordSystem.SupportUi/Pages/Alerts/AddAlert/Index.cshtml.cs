using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), ActivatesJourney, RequireJourneyInstance]
public class IndexModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ReferenceDataCache _referenceDataCache;

    public IndexModel(
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

    public AlertType[]? AlertTypes { get; set; }

    public void OnGet()
    {
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var person = await _crmQueryDispatcher.ExecuteQuery(new GetContactDetailByIdQuery(PersonId, new ColumnSet(Contact.Fields.Id)));
        if (person is null)
        {
            context.Result = NotFound();
            return;
        }

        var sanctionCodes = await _referenceDataCache.GetSanctionCodes();
        AlertTypes = sanctionCodes
            .Select(MapSanctionCode)
            .OrderBy(a => a.Name)
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    private AlertType MapSanctionCode(dfeta_sanctioncode sanctionCode) =>
        new AlertType()
        {
            AlertTypeId = sanctionCode.dfeta_sanctioncodeId!.Value,
            Value = sanctionCode.dfeta_Value,
            Name = sanctionCode.dfeta_name
        };
}
