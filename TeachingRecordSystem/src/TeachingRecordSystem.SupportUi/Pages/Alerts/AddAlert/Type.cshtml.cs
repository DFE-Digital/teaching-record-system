using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class TypeModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Select an alert type")]
    [Required(ErrorMessage = "Select an alert type")]
    public Guid? AlertTypeId { get; set; }

    public AlertCategoryInfo[]? Categories { get; set; }

    public AlertTypeInfo[]? AlertTypes { get; set; }

    public void OnGet()
    {
        AlertTypeId = JourneyInstance!.State.AlertTypeId;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var selectedType = AlertTypes!.Single(t => t.AlertTypeId == AlertTypeId);

        await JourneyInstance!.UpdateStateAsync(
            state =>
            {
                state.AlertTypeId = AlertTypeId;
                state.AlertTypeName = selectedType.Name;
            });

        return Redirect(FromCheckAnswers
            ? linkGenerator.AlertAddCheckAnswers(PersonId, JourneyInstance.InstanceId)
            : linkGenerator.AlertAddDetails(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;

        var alertTypes = await referenceDataCache.GetAlertTypes(activeOnly: true);
        AlertTypes = alertTypes.Select(t => new AlertTypeInfo(t.AlertTypeId, t.AlertCategoryId, t.Name, t.DisplayOrder)).ToArray();

        var categories = await referenceDataCache.GetAlertCategories();
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