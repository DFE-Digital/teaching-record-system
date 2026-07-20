using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.SupportTasks;

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
            .WithMessage("Select a user to assign the tasks to")
    };

    [FromQuery(Name = "SupportTaskReference")]
    public string[]? SupportTaskReferences { get; set; }

    public IReadOnlyCollection<TaskInfo>? Tasks { get; set; }

    public IReadOnlyCollection<AssignableUserInfo>? AssignToOptions { get; set; }

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
        var userName = AssignToOptions!.SingleOrDefault(a => a.UserId == AssignToUserId)?.UserName;
        if (userName is null)
        {
            return BadRequest();
        }

        var processContext = new ProcessContext(ProcessType.SupportTasksAssigning, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.AssignSupportTasksAsync(
            new AssignSupportTasksOptions
            {
                SupportTaskReferences = Tasks!.Select(t => t.SupportTaskReference),
                UserId = AssignToUserId!.Value
            },
            processContext);

        TempData.SetFlashNotificationBanner(
            $"{Tasks!.Count} tasks assigned to {userName}");

        return Redirect(linkGenerator.SupportTasks.Active());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (SupportTaskReferences?.Length is not > 0)
        {
            context.Result = BadRequest();
            return;
        }

        // Explicit SQL query for the 'for update' addition
        Tasks = await dbContext.SupportTasks.FromSql(
                $"select * from support_tasks where support_task_reference = any({SupportTaskReferences}) and deleted_on is null for update")
            .Where(t => t.IsOutstanding)  // Exclude any tasks that may have been Completed since the initial selection
            .OrderBy(t => t.CreatedOn)
            .Select(t => new TaskInfo(t.SupportTaskReference, t.CreatedOn, t.SubjectName, t.SubjectEmailAddress, t.AssignedTo != null ? t.AssignedTo.Name : null, t.SupportTaskType, t.Status))
            .ToArrayAsync();

        if (Tasks.Count == 0)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.Active());
            return;
        }

        AssignToOptions = await supportTaskService.GetAssignableUsersAsync();

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
