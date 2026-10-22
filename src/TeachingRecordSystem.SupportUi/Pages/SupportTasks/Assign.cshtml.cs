using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks;

public class Assign(
    SupportTaskService supportTaskService,
    IOptions<SupportTaskAssignmentOptions> assignmentOptions,
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

    private const string SupportTaskReferenceQueryParameterName = "SupportTaskReference";

    [FromQuery(Name = SupportTaskReferenceQueryParameterName)]
    public string[]? SupportTaskReferences { get; set; }

    // How the list the user picked from was sorted. The selection can span several pages, so the
    // order the references arrive in isn't the order they were listed in - only re-applying the sort
    // gets the tasks back into the order the user saw.
    [FromQuery]
    public SupportTasksSortByOption? SortBy { get; set; }

    [FromQuery]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ShowMyTeamMembersOnly { get; set; }

    [FromQuery]
    public TeamWorkloadSortByOption? TeamWorkloadSortBy { get; set; }

    [FromQuery]
    public SortDirection? TeamWorkloadSortDirection { get; set; }

    [FromQuery]
    public bool ExpandTeamWorkload { get; set; }

    public IReadOnlyCollection<TaskInfo>? Tasks { get; set; }

    public IReadOnlyCollection<AssignableUserInfo>? AssignToOptions { get; set; }

    public bool ShowMyselfOption { get; set; }

    public Guid? CurrentUserTeamId { get; set; }

    public bool HasAssignableUsersOutsideCurrentUsersTeam { get; set; }

    public bool ShowTeamMembersFilter => CurrentUserTeamId.HasValue && HasAssignableUsersOutsideCurrentUsersTeam;

    public bool ShowTeamWorkload => CurrentUserTeamId.HasValue;

    public IReadOnlyCollection<TeamWorkloadMemberInfo> TeamWorkloadMembers { get; set; } = [];

    public Guid UnassignedUserId => SupportTaskSearchService.UnassignedUserId;

    public Guid CurrentUserId => User.GetUserId();

    public string ToggleShowMyTeamMembersOnlyUrl =>
        GetAssignPageUrl(showMyTeamMembersOnly: !ShowMyTeamMembersOnly);

    [BindProperty]
    public Guid? AssignToUserId { get; set; }

    // Where the back and cancel links go: the task list the user came from, with their selection
    // intact. They may have built it up over several pages, so dropping it would mean picking every
    // task out again.
    public string? BackLink { get; set; }

    // Where to go once the tasks have been assigned: the same list, but with nothing selected.
    public string? ReturnUrl { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await this.ThrowIfInvalidAsync(_validator, cancellationToken);

        // Belt & braces check that user assignment is valid
        Guid[] extraAssignmentIds = ShowMyselfOption ? [CurrentUserId, UnassignedUserId] : [UnassignedUserId];
        var validAssignmentIds = AssignToOptions!.Select(u => u.UserId).Concat(extraAssignmentIds);
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
            processContext,
            cancellationToken);

        var taskCountMessage = $"{Tasks!.Count} task{(Tasks.Count is 1 ? "" : "s")}";
        TempData.SetFlashNotificationBanner(
            AssignToUserId == UnassignedUserId
                ? $"{taskCountMessage} unassigned"
                : $"{taskCountMessage} assigned to " +
                    $"{(AssignToUserId == CurrentUserId ? "you" : AssignToOptions!.Single(a => a.UserId == AssignToUserId).UserName)}");

        return Redirect(ReturnUrl!);
    }

    public string GetTeamWorkloadSortUrl(TeamWorkloadSortByOption sortBy, SortDirection sortDirection) =>
        GetAssignPageUrl(
            showMyTeamMembersOnly: ShowMyTeamMembersOnly,
            teamWorkloadSortBy: sortBy,
            teamWorkloadSortDirection: sortDirection,
            expandTeamWorkload: true);

    public bool IsSortedBy(TeamWorkloadSortByOption sortBy) =>
        (TeamWorkloadSortBy ?? TeamWorkloadSortByOption.TeamMember) == sortBy;

    public bool IsTeamWorkloadSortedAscending() =>
        (TeamWorkloadSortDirection ?? SupportUi.SortDirection.Ascending) == SupportUi.SortDirection.Ascending;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var cancellationToken = context.HttpContext.RequestAborted;

        if (SupportTaskReferences?.Length is not > 0)
        {
            context.Result = BadRequest();
            return;
        }

        // Explicit SQL query for the 'for update' addition
        var tasks = await dbContext.SupportTasks.FromSql(
                $"select * from support_tasks where support_task_reference = any({SupportTaskReferences}) and deleted_on is null for update")
            .Where(t => t.IsOutstanding)  // Exclude any tasks that may have been Completed since the initial selection
            .OrderBySupportTasksSortOption(
                SortBy ?? SupportTasksSortByOption.RequestedOn,
                SortDirection ?? SupportUi.SortDirection.Ascending)
            .Select(t => new TaskInfo(
                t.SupportTaskReference,
                t.CreatedOn,
                t.SubjectName,
                t.SubjectEmailAddress,
                t.AssignedTo != null ? t.AssignedTo.Name : null,
                t.SupportTaskType,
                t.Status,
                t.SourceApplicationUser!.ShortName ?? t.SourceApplicationUser.Name))
            .ToArrayAsync(cancellationToken);

        Tasks = tasks.AsReadOnly();

        if (Tasks.Count == 0)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.Active());
            return;
        }

        var assignableUsers = await supportTaskService.GetAssignableUsersAsync(
            includeAdministrators: assignmentOptions.Value.IncludeAdministrators,
            includeCurrentAssignees: false,
            cancellationToken: cancellationToken);

        ShowMyselfOption = assignableUsers.Any(u => u.UserId == CurrentUserId);

        CurrentUserTeamId = await dbContext.Users
            .Where(u => u.UserId == CurrentUserId)
            .Select(u => u.TeamId)
            .SingleAsync(cancellationToken);

        if (!CurrentUserTeamId.HasValue)
        {
            ShowMyTeamMembersOnly = false;
        }

        var assignableUserIds = assignableUsers.Select(u => u.UserId).ToArray();
        var userTeams = await dbContext.Users
            .Where(u => assignableUserIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.TeamId })
            .ToDictionaryAsync(u => u.UserId, u => u.TeamId, cancellationToken);

        var assignableOptions = assignableUsers
            .Where(u => u.UserId != CurrentUserId)
            .Select(u =>
            {
                var teamId = userTeams[u.UserId];
                var isInCurrentUsersTeam = CurrentUserTeamId.HasValue && teamId == CurrentUserTeamId;

                return new AssignableUserOption(u, isInCurrentUsersTeam);
            })
            .ToArray();

        HasAssignableUsersOutsideCurrentUsersTeam =
            CurrentUserTeamId.HasValue && assignableOptions.Any(o => !o.IsInCurrentUsersTeam);

        if (ShowMyTeamMembersOnly)
        {
            assignableOptions = assignableOptions.Where(o => o.IsInCurrentUsersTeam).ToArray();
        }

        assignableOptions = [.. assignableOptions
            .OrderByDescending(o => o.IsInCurrentUsersTeam)
            .ThenBy(o => o.AssignableUser.UserName)];

        AssignToOptions = assignableOptions
            .Select(o => o.AssignableUser)
            .ToArray()
            .AsReadOnly();

        if (CurrentUserTeamId.HasValue)
        {
            await SetTeamWorkloadMembersAsync(cancellationToken);
        }

        ReturnUrl = this.GetReturnUrlOrDefault(linkGenerator.SupportTasks.Active());

        BackLink = QueryHelpers.AddQueryString(
            ReturnUrl,
            new Dictionary<string, StringValues>
            {
                { SupportTaskReferenceQueryParameterName, Tasks.Select(t => t.SupportTaskReference).ToArray() }
            });

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    private string GetAssignPageUrl(
        bool? showMyTeamMembersOnly = null,
        TeamWorkloadSortByOption? teamWorkloadSortBy = null,
        SortDirection? teamWorkloadSortDirection = null,
        bool? expandTeamWorkload = null)
    {
        var effectiveShowMyTeamMembersOnly = showMyTeamMembersOnly ?? ShowMyTeamMembersOnly;
        var effectiveTeamWorkloadSortBy = teamWorkloadSortBy ?? TeamWorkloadSortBy;
        var effectiveTeamWorkloadSortDirection = teamWorkloadSortDirection ?? TeamWorkloadSortDirection;
        var effectiveExpandTeamWorkload = expandTeamWorkload ?? ExpandTeamWorkload;

        var query = new Dictionary<string, StringValues>
        {
            { SupportTaskReferenceQueryParameterName, Tasks!.Select(t => t.SupportTaskReference).ToArray() }
        };

        if (SortBy is not null)
        {
            query.Add(nameof(SortBy), SortBy.ToString());
        }

        if (SortDirection is not null)
        {
            query.Add(nameof(SortDirection), SortDirection.ToString());
        }

        if (effectiveShowMyTeamMembersOnly)
        {
            query.Add(nameof(ShowMyTeamMembersOnly), bool.TrueString);
        }

        if (effectiveTeamWorkloadSortBy is not null)
        {
            query.Add(nameof(TeamWorkloadSortBy), effectiveTeamWorkloadSortBy.ToString());
        }

        if (effectiveTeamWorkloadSortDirection is not null)
        {
            query.Add(nameof(TeamWorkloadSortDirection), effectiveTeamWorkloadSortDirection.ToString());
        }

        if (effectiveExpandTeamWorkload)
        {
            query.Add(nameof(ExpandTeamWorkload), bool.TrueString);
        }

        if (Request.Query.TryGetValue("returnUrl", out var returnUrl) && !StringValues.IsNullOrEmpty(returnUrl))
        {
            query.Add("returnUrl", returnUrl);
        }

        return QueryHelpers.AddQueryString("/support-tasks/assign", query);
    }

    private async Task SetTeamWorkloadMembersAsync(CancellationToken cancellationToken)
    {
        var teamMembers = await dbContext.Users
            .Where(u =>
                u.TeamId == CurrentUserTeamId &&
                u.Active &&
                (u.Role == UserRoles.AccessManager ||
                 u.Role == UserRoles.RecordManager ||
                 (assignmentOptions.Value.IncludeAdministrators && u.Role == UserRoles.Administrator)))
            .Select(u => new { u.UserId, u.Name })
            .ToArrayAsync(cancellationToken);

        if (teamMembers.Length == 0)
        {
            TeamWorkloadMembers = [];
            return;
        }

        var teamMemberIds = teamMembers.Select(m => m.UserId).ToArray();

        var supportTasksByTypeAndMember = await dbContext.SupportTasks
            .Where(t => t.IsOutstanding && t.AssignedToUserId.HasValue && teamMemberIds.Contains(t.AssignedToUserId.Value))
            .GroupBy(t => new { UserId = t.AssignedToUserId!.Value, t.SupportTaskType })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.SupportTaskType,
                Count = g.Count()
            })
            .ToArrayAsync(cancellationToken);

        var taskCountsByMember = supportTasksByTypeAndMember
            .GroupBy(c => c.UserId)
            .ToDictionary(g => g.Key, g => g
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.SupportTaskType.GetTitle())
                .Select(c => new TeamWorkloadByTaskType(c.SupportTaskType, c.Count))
                .ToArray());

        var members = teamMembers
            .Select(m =>
            {
                var taskBreakdown = taskCountsByMember.GetValueOrDefault(m.UserId, []);

                return new TeamWorkloadMemberInfo(
                    m.UserId,
                    m.Name,
                    taskBreakdown.Sum(t => t.TaskCount),
                    taskBreakdown);
            })
            .ToArray();

        var sortBy = TeamWorkloadSortBy ?? TeamWorkloadSortByOption.TeamMember;
        var sortDirection = TeamWorkloadSortDirection ?? SupportUi.SortDirection.Ascending;

        TeamWorkloadMembers = (sortBy, sortDirection) switch
        {
            (TeamWorkloadSortByOption.Tasks, SupportUi.SortDirection.Descending) =>
                [.. members.OrderByDescending(m => m.TaskCount).ThenBy(m => m.UserName)],
            (TeamWorkloadSortByOption.Tasks, _) =>
                [.. members.OrderBy(m => m.TaskCount).ThenBy(m => m.UserName)],
            (_, SupportUi.SortDirection.Descending) =>
                [.. members.OrderByDescending(m => m.UserName)],
            _ =>
                [.. members.OrderBy(m => m.UserName)]
        };
    }

    public record TaskInfo(
        string SupportTaskReference,
        DateTime CreatedOn,
        string? SubjectName,
        string? SubjectEmailAddress,
        string? AssignedTo,
        SupportTaskType Type,
        SupportTaskStatus Status,
        string? SourceApplicationName)
    {
        public string Subject => (SubjectName ?? SubjectEmailAddress)!;
    }

    public enum TeamWorkloadSortByOption
    {
        TeamMember,
        Tasks
    }

    public record TeamWorkloadMemberInfo(
        Guid UserId,
        string UserName,
        int TaskCount,
        IReadOnlyCollection<TeamWorkloadByTaskType> TaskBreakdown);

    public record TeamWorkloadByTaskType(SupportTaskType TaskType, int TaskCount);

    private record AssignableUserOption(AssignableUserInfo AssignableUser, bool IsInCurrentUsersTeam);
}
