using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

public class ZendeskTickets(
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator,
    SupportTaskService supportTaskService) : PageModel
{
    private readonly InlineValidator<ZendeskTickets> _validator = new()
    {
        v => v.RuleForEach(m => m.TicketUrls!)
            .Where(ticket => !string.IsNullOrWhiteSpace(ticket))
            .ZendeskUrl("Enter a valid Zendesk URL")
            .When(m => m.TicketUrls is not null)
    };

    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public string SupportTaskTypeTitle { get; set; } = null!;

    public SupportTaskType SupportTaskType { get; set; }

    [BindProperty]
    public List<string>? TicketUrls { get; set; }

    public string? BackLink { get; set; }

    public void OnGet()
    {
        TicketUrls = _supportTask!.ZendeskTickets.ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await this.ThrowIfInvalidAsync(_validator);

        var sanitizedTicketUrls = (TicketUrls ?? []).Where(url => !string.IsNullOrEmpty(url));

        var processContext = new ProcessContext(ProcessType.SupportTaskZendeskUrlsUpdating, timeProvider.UtcNow, SystemUser.SystemUserId);

        var updated = await supportTaskService.UpdateZendeskUrlsAsync(
            new UpdateZendeskUrlsOptions
            {
                SupportTaskReference = SupportTaskReference,
                ZendeskUrls = sanitizedTicketUrls
            },
            processContext);

        if (updated)
        {
            TempData.SetFlashNotificationBanner("Zendesk tickets updated");
        }

        return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(
            SupportTaskReference,
            expandZendeskTickets: true,
            returnUrl: this.GetReturnUrl()));
    }

    public IActionResult OnPostRemoveTicket(int index)
    {
        TicketUrls!.RemoveAt(index);

        if (TicketUrls.Count == 0)
        {
            TicketUrls.Add(string.Empty);
        }

        return Page();
    }

    public IActionResult OnPostAddTicket()
    {
        TicketUrls!.Add(string.Empty);

        return Page();
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        _supportTask = HttpContext
            .GetCurrentSupportTaskFeature()
            .SupportTask;

        SupportTaskType = _supportTask.SupportTaskType;
        SupportTaskTypeTitle = _supportTask.SupportTaskType.GetTitle();

        BackLink = this.GetReturnUrlOrDefault(linkGenerator.SupportTasks.SupportTaskDetail.Index(SupportTaskReference));
    }
}
