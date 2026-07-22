using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

[Journey(JourneyNames.EditAlertLink), StartsJourney]
public class IndexModel(
    EditAlertLinkJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.AddLink)
            .NotNull()
            .WithMessage(m => string.IsNullOrEmpty(m.PreviousLink)
                ? "Select yes if you want to add a link to a panel outcome"
                : "Select change link if you want to change the link to the panel outcome"),
        v => v.RuleFor(m => m.Link)
            .Cascade(CascadeMode.Stop)
            .Must(link => TrsUriHelper.TryCreateWebsiteUri(link, out _)).WithMessage("Enter a valid URL")
            .Must((m, link) => link != m.PreviousLink).WithMessage("Enter a different link")
            .When(m => m.AddLink == true)
    };

    public JourneyInstanceId InstanceId => journey.InstanceId;

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public bool? AddLink { get; set; }

    [BindProperty]
    public string? Link { get; set; }

    public string? PreviousLink { get; set; }

    public void OnGet()
    {
        AddLink = journey.State.AddLink;
        Link = journey.State.Link;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return await CancelAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        // There was no link to begin with and the user doesn't want to add one, so there's nothing to
        // change; abandon the journey and return to the record.
        if (string.IsNullOrEmpty(PreviousLink) && AddLink == false)
        {
            return await CancelAsync();
        }

        return journey.AdvanceTo(
            linkGenerator.Alerts.EditAlert.Link.Reason(journey.InstanceId),
            state =>
            {
                state.AddLink = AddLink;
                state.Link = AddLink == true ? Link : null;
            });
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(journey.State.Evidence.UploadedEvidenceFile);
        journey.DeleteInstance();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        PreviousLink = alertInfo.Alert.ExternalLink;

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Alerts(PersonId);
    }
}
