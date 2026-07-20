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
            .Must(ticket =>
                string.IsNullOrWhiteSpace(ticket) ||
                IsValidZendeskUrl(ticket))
            .WithMessage("Enter a valid Zendesk URL")
            .When(m => m.TicketUrls is not null)
    };

    [FromRoute] public string SupportTaskReference { get; set; } = null!;

    public string SupportTaskTypeTitle { get; set; } = null!;

    public SupportTaskType SupportTaskType { get; set; }

    [BindProperty] public List<string>? TicketUrls { get; set; }

    private SupportTask? _supportTask { get; set; }

    public void OnGet()
    {
        var supportTask = HttpContext
            .GetCurrentSupportTaskFeature()
            .SupportTask;

        TicketUrls = supportTask.ZendeskTickets?.ToList() ?? [];
    }

    public override void OnPageHandlerExecuting(
        PageHandlerExecutingContext context)
    {
        _supportTask = HttpContext
            .GetCurrentSupportTaskFeature()
            .SupportTask;

        SupportTaskType = _supportTask.SupportTaskType;
        SupportTaskTypeTitle = _supportTask.SupportTaskType.GetTitle();
    }

    public IActionResult OnPostAddTicket()
    {
        TicketUrls ??= [];
        TicketUrls.Add(string.Empty);

        ModelState.Clear();

        return Page();
    }

    public IActionResult OnPostRemoveTicket(int removeIndex)
    {
        TicketUrls ??= [];

        if (removeIndex >= 0 && removeIndex < TicketUrls.Count)
        {
            TicketUrls.RemoveAt(removeIndex);
        }

        foreach (var key in ModelState.Keys
                     .Where(k => k.StartsWith(nameof(TicketUrls) + "["))
                     .ToList())
        {
            ModelState.Remove(key);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        var processContext = new ProcessContext(ProcessType.SupportTaskZendeskUrlsUpdating, timeProvider.UtcNow, SystemUser.SystemUserId);

        await supportTaskService.UpdateZendeskUrlsAsync(SupportTaskReference, TicketUrls?.ToArray() ?? [], processContext);

        TempData.SetFlashNotificationBanner(
            "Zendesk tickets updated");

        return Redirect(
            linkGenerator.SupportTasks.SupportTaskDetail.Index(
                SupportTaskReference));
    }

    private static bool IsValidZendeskUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(
                uri.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!uri.Host.EndsWith(
                "zendesk.com",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.IsNullOrEmpty(uri.AbsolutePath)
               && uri.AbsolutePath != "/";
    }
}
