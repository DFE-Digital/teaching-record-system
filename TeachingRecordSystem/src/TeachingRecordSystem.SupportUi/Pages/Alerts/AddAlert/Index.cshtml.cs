using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    private readonly SanctionTextLookup _sanctionTextLookup;

    public IndexModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache,
        SanctionTextLookup sanctionTextLookup)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
        _referenceDataCache = referenceDataCache;
        _sanctionTextLookup = sanctionTextLookup;
    }

    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [BindProperty]
    [Display(Name = "Alert type")]
    public Guid? AlertTypeId { get; set; }

    [BindProperty]
    [Display(Description = "You can enter up to 4000 characters")]
    public string? Details { get; set; }

    [BindProperty]
    [Display(Description = "Optional")]
    public string? Link { get; set; }

    [BindProperty]
    [Display(Name = "Start date")]
    public DateOnly? StartDate { get; set; }

    public AlertType[]? AlertTypes { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (AlertTypeId is null)
        {
            ModelState.AddModelError(nameof(AlertTypeId), "Add an alert type");
        }

        if (string.IsNullOrWhiteSpace(Details))
        {
            ModelState.AddModelError(nameof(Details), "Add details");
        }

        if (!string.IsNullOrEmpty(Link) &&
            (!Uri.TryCreate(Link, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https")))
        {
            ModelState.AddModelError(nameof(Link), "Enter a valid URL");
        }

        if (StartDate is null)
        {
            ModelState.AddModelError(nameof(StartDate), "Add a start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s =>
        {
            s.AlertTypeId = AlertTypeId;
            s.Details = Details;
            s.Link = Link;
            s.StartDate = StartDate;
        });

        return Redirect(_linkGenerator.AlertAddConfirm(PersonId, JourneyInstance!.InstanceId));
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

        AlertTypeId ??= JourneyInstance!.State.AlertTypeId;
        Details ??= JourneyInstance!.State.Details;
        Link ??= JourneyInstance!.State.Link;
        StartDate ??= JourneyInstance!.State.StartDate;

        await next();
    }

    private AlertType MapSanctionCode(dfeta_sanctioncode sanctionCode) =>
        new AlertType()
        {
            AlertTypeId = sanctionCode.dfeta_sanctioncodeId!.Value,
            Value = sanctionCode.dfeta_Value,
            Name = sanctionCode.dfeta_name,
            DefaultText = _sanctionTextLookup.GetSanctionDefaultText(sanctionCode.dfeta_Value) ?? string.Empty
        };
}
