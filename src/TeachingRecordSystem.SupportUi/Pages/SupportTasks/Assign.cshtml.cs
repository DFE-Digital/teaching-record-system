using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class Assign(
    SupportTaskService supportTaskService,
    TrsDbContext dbContext,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) :
    PageModel
{
    private readonly InlineValidator<Assign> _validator = new()
    {
        v => v
            .RuleFor(m => m.AssignToUserId)
            .NotNull()
            .WithMessage("Select who to assign the tasks to")
    };

    [FromQuery(Name = "SupportTaskReference")]
    public string[]? SupportTaskReferences { get; set; }

    public IReadOnlyCollection<TaskInfo>? Tasks { get; set; }

    public IReadOnlyCollection<AssignableUserInfo>? AssignToOptions { get; set; }

    public Guid UnassignedUserId => SupportTaskSearchService.UnassignedUserId;

    public Guid CurrentUserId => User.GetUserId();

    [BindProperty]
    public Guid? AssignToUserId { get; set; }

    public string? BackLink { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _validator.ValidateAndThrowAsync(this);

        // Belt & braces check that user assignment is valid
        var validAssignmentIds = AssignToOptions!.Select(u => u.UserId).Concat([CurrentUserId, UnassignedUserId]);
        if (!validAssignmentIds.Contains(AssignToUserId!.Value))
        {
            return BadRequest();
        }

        var processContext = new ProcessContext(ProcessType.SupportTasksAssigning, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.AssignSupportTasksAsync(
            new AssignSupportTasksOptions
            {
                SupportTaskReferences = Tasks!.Select(t => t.SupportTaskReference),
                UserId = AssignToUserId == UnassignedUserId ? null : AssignToUserId
            },
            processContext);

        var taskCountMessage = $"{Tasks!.Count} task{(Tasks.Count is 1 ? "" : "s")}";
        TempData.SetFlashNotificationBanner(
            AssignToUserId == UnassignedUserId
                ? $"{taskCountMessage} unassigned"
                : $"{taskCountMessage} assigned to " +
                    $"{(AssignToUserId == CurrentUserId ? "you" : AssignToOptions!.Single(a => a.UserId == AssignToUserId).UserName)}");

        return Redirect(BackLink!);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (SupportTaskReferences?.Length is not > 0)
        {
            context.Result = BadRequest();
            return;
        }

        // Explicit SQL query for the 'for update' addition
        var tasks = await dbContext.SupportTasks.FromSql(
                $"select * from support_tasks where support_task_reference = any({SupportTaskReferences}) and deleted_on is null for update")
            .Where(t => t.IsOutstanding)  // Exclude any tasks that may have been Completed since the initial selection
            .OrderBy(t => t.CreatedOn)
            .Select(t => new TaskInfo(t.SupportTaskReference, t.CreatedOn, t.SubjectName, t.SubjectEmailAddress, t.AssignedTo != null ? t.AssignedTo.Name : null, t.SupportTaskType, t.Status))
            .ToArrayAsync();

        // Re-order so that tasks are in the order they were specified in
        Tasks = tasks.OrderBy(t => SupportTaskReferences.IndexOf(t.SupportTaskReference)).AsReadOnly();

        if (Tasks.Count == 0)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.Active());
            return;
        }

        AssignToOptions = (await supportTaskService.GetAssignableUsersAsync())
            .Where(u => u.UserId != CurrentUserId)
            .AsReadOnly();

        BackLink = this.GetReturnUrlOrDefault(linkGenerator.SupportTasks.Active());

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record TaskInfo(
        string SupportTaskReference,
        DateTime CreatedOn,
        string? SubjectName,
        string? SubjectEmailAddress,
        string? AssignedTo,
        SupportTaskType Type,
        SupportTaskStatus Status)
    {
        public string Subject => (SubjectName ?? SubjectEmailAddress)!;
    }
}
