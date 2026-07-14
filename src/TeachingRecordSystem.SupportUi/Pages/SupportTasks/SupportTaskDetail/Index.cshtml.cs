using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

[AllowClosedSupportTask]
public class Index(
    SupportTaskService supportTaskService,
    TrsDbContext dbContext,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) :
    PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public string? Subject { get; set; }

    public SupportTaskType SupportTaskType { get; set; }

    public string? SupportTaskTypeTitle { get; set; }

    public string? SubjectLink { get; set; }

    public string? SubjectLinkText { get; set; }

    public bool IsOutstanding { get; set; }

    public string? OutcomeLabel { get; set; }

    public DateTime? CompletedOn { get; set; }

    public string? CompletedByUserName { get; set; }

    public IReadOnlyCollection<Note>? Notes { get; set; }

    [BindProperty]
    public Guid? AssignedToUserId { get; set; }

    [BindProperty]
    public SupportTaskStatus Status { get; set; }

    [FromQuery]
    public bool ExpandNotes { get; set; }

    public IReadOnlyCollection<AssignableUserInfo>? AssignToOptions { get; set; }

    public void OnGet()
    {
        AssignedToUserId = _supportTask!.AssignedToUserId;
        Status = _supportTask.Status;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_supportTask!.IsOutstanding)
        {
            return BadRequest();
        }

        // Belt & braces check that status and user assignments are valid
        if (Status is not SupportTaskStatus.InProgress and not SupportTaskStatus.Open ||
            AssignedToUserId is not null && !AssignToOptions!.Any(o => o.UserId == AssignedToUserId))
        {
            return BadRequest();
        }

        var processContext = new ProcessContext(ProcessType.SupportTaskAllocating, timeProvider.UtcNow, User.GetUserId());

        var updated = await supportTaskService.AllocateSupportTaskAsync(
            new AllocateSupportTaskOptions
            {
                SupportTaskReference = SupportTaskReference,
                Status = Status,
                AssignToUserId = AssignedToUserId
            },
            processContext);

        if (updated)
        {
            TempData.SetFlashNotificationBanner("Task updated");
        }

        return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(SupportTaskReference));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        Subject = _supportTask.GetSubject();
        SupportTaskType = _supportTask.SupportTaskType;
        SupportTaskTypeTitle = _supportTask.SupportTaskType.GetTitle();
        IsOutstanding = _supportTask.IsOutstanding;
        OutcomeLabel = _supportTask.OutcomeLabel;
        CompletedOn = _supportTask.CompletedOn;
        CompletedByUserName = _supportTask.CompletedBy?.Name;

        (SubjectLink, SubjectLinkText) = _supportTask.PersonId is Guid personId ? (linkGenerator.Persons.PersonDetail.Index(personId), "View record") :
            _supportTask.TrnRequestMetadata?.ResolvedPersonId is Guid resolvedPersonId ? (linkGenerator.Persons.PersonDetail.Index(resolvedPersonId), "View record") :
            _supportTask.OneLoginUserSubject is string oneLoginUserSubject ? (linkGenerator.OneLogins.OneLoginDetail.Index(oneLoginUserSubject), "View One Login") :
            (null, null);

        Notes = await dbContext.SupportTaskNotes
            .Where(t => t.SupportTaskReference == SupportTaskReference)
            .OrderByDescending(t => t.CreatedOn)
            .Select(t => new Note(t.Content, t.CreatedOn, t.CreatedBy!.Name))
            .ToArrayAsync();

        AssignToOptions = await supportTaskService.GetAssignableUsersAsync();

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record Note(string Content, DateTime CreatedOn, string CreatedBy);
}
