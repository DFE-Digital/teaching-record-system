using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert)]
public class TypeModel(
    AddAlertJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager,
    IAuthorizationService authorizationService) : PageModel
{
    private readonly InlineValidator<TypeModel> _validator = new()
    {
        v => v.RuleFor(m => m.AlertTypeId).AlertType(requiredMessage: "Select an alert type")
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public Guid? AlertTypeId { get; set; }

    public AlertCategoryInfo[]? Categories { get; set; }

    public AlertTypeInfo[]? AlertTypes { get; set; }

    public void OnGet()
    {
        AlertTypeId = journey.State.AlertTypeId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        var selectedType = AlertTypes!.Single(t => t.AlertTypeId == AlertTypeId);

        return journey.AdvanceTo(
            linkGenerator.Alerts.AddAlert.Details(journey.InstanceId),
            state =>
            {
                state.AlertTypeId = AlertTypeId;
                state.AlertTypeName = selectedType.Name;
            });
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;

        var alertTypes = await referenceDataCache.GetAlertTypesAsync(activeOnly: true);
        AlertTypes = await alertTypes
            .ToAsyncEnumerable()
            .Where(async (t, _) => (await authorizationService.AuthorizeAsync(User, t.AlertTypeId, new AlertTypePermissionRequirement(Permissions.Alerts.Write))) is { Succeeded: true })
            .Select(t => new AlertTypeInfo(t.AlertTypeId, t.AlertCategoryId, t.Name, t.DisplayOrder ?? int.MaxValue))
            .ToArrayAsync();

        var categories = await referenceDataCache.GetAlertCategoriesAsync();
        Categories = categories.Select(
            c => new AlertCategoryInfo(
                c.Name,
                c.DisplayOrder,
                AlertTypes.Where(t => t.AlertCategoryId == c.AlertCategoryId).OrderBy(t => t.DisplayOrder).ToArray()))
            .Where(c => c.AlertTypes.Length > 0)
            .OrderBy(c => c.DisplayOrder)
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record AlertTypeInfo(Guid AlertTypeId, Guid AlertCategoryId, string Name, int DisplayOrder);

    public record AlertCategoryInfo(string Name, int DisplayOrder, AlertTypeInfo[] AlertTypes);
}
