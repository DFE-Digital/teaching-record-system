using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Shared.Components.ChangeHistoryEntry;
using TeachingRecordSystem.SupportUi.Services.ChangeHistory;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.SupportTaskDetail;

[AllowClosedSupportTask]
public class Index(
    SupportTaskService supportTaskService,
    IOptions<SupportTaskAssignmentOptions> assignmentOptions,
    ChangeHistoryService changeHistoryService,
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

    public SupportTaskOutcome? Outcome { get; set; }

    public DateTime? CompletedOn { get; set; }

    public string? CompletedByUserName { get; set; }

    public IReadOnlyCollection<Note>? Notes { get; set; }

    public IReadOnlyCollection<string>? ZendeskTickets { get; set; }

    public string? BackLink { get; set; }

    [BindProperty]
    public Guid? AssignedToUserId { get; set; }

    [BindProperty]
    public SupportTaskStatus Status { get; set; }

    [FromQuery]
    public bool ExpandNotes { get; set; }

    [FromQuery]
    public bool ExpandZendeskTickets { get; set; }

    public IReadOnlyCollection<AssignableUserInfo>? AssignToOptions { get; set; }

    public bool ShowMyselfOption { get; set; }

    public Guid UnassignedUserId => SupportTaskSearchService.UnassignedUserId;

    public Guid CurrentUserId => User.GetUserId();

    public IReadOnlyCollection<ChangeHistoryEntryViewModel>? ChangeHistory { get; set; }

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
        Guid[] extraAssignmentIds = ShowMyselfOption ? [CurrentUserId, UnassignedUserId] : [UnassignedUserId];
        var validAssignmentIds = AssignToOptions!.Select(u => u.UserId).Concat(extraAssignmentIds);
        if ((Status is not SupportTaskStatus.InProgress and not SupportTaskStatus.Open) ||
            (AssignedToUserId is not null && !validAssignmentIds.Contains(AssignedToUserId.Value)))
        {
            return BadRequest();
        }

        var processContext = new ProcessContext(ProcessType.SupportTaskAllocating, timeProvider.UtcNow, User.GetUserId());

        var updated = await supportTaskService.AllocateSupportTaskAsync(
            new AllocateSupportTaskOptions
            {
                SupportTaskReference = SupportTaskReference,
                Status = Status,
                AssignToUserId = AssignedToUserId == UnassignedUserId ? null : AssignedToUserId
            },
            processContext);

        if (updated)
        {
            TempData.SetFlashNotificationBanner("Task updated");
        }

        return Redirect(Request.GetEncodedPathAndQuery());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        Subject = _supportTask.GetSubject();
        SupportTaskType = _supportTask.SupportTaskType;
        SupportTaskTypeTitle = _supportTask.SupportTaskType.GetTitle();
        IsOutstanding = _supportTask.IsOutstanding;
        Outcome = _supportTask.Outcome;
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

        ZendeskTickets = _supportTask.ZendeskTickets;

        // The user the task is already assigned to may not be assignable any more; include them anyway so the
        // 'Assigned to' select shows who has it rather than falling back to 'Unassigned'.
        var assignableUsers = await supportTaskService.GetAssignableUsersAsync(
            includeAdministrators: assignmentOptions.Value.IncludeAdministrators,
            includeCurrentAssignees: false,
            includeUserId: _supportTask.AssignedToUserId);

        ShowMyselfOption = assignableUsers.Any(u => u.UserId == CurrentUserId);

        AssignToOptions = assignableUsers
            .Where(u => u.UserId != CurrentUserId)
            .AsReadOnly();

        BackLink = this.GetReturnUrlOrDefault(
            IsOutstanding ? linkGenerator.SupportTasks.Active() : linkGenerator.SupportTasks.Completed());

        ChangeHistory = (await changeHistoryService.GetChangeHistoryBySupportTaskAsync(SupportTaskReference))
            .Select(e => new ChangeHistoryEntryViewModel
            {
                Context = e.Context,
                Timestamp = e.Process.CreatedOn,
                UserName = e.RaisedByUser.Name,
                ProcessId = e.Process.ProcessId,
                ProcessType = e.Process.ProcessType,
                ChangeReason = e.Process.ChangeReason,
                Events = e.Process.Events!.Select(v => v.Payload).AsReadOnly(),
            })
            .AsReadOnly();

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public static string GetZendeskTicketDisplayText(string url) => new Uri(url).LocalPath;

    public record Note(string Content, DateTime CreatedOn, string CreatedBy);
}
