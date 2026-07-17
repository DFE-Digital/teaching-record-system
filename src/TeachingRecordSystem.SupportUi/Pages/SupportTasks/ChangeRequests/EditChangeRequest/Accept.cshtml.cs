using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.SupportTasksEdit)]
public class AcceptModel(
    ChangeRequestSupportTaskService changeRequestSupportTaskService,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public SupportTaskType ChangeType { get; set; }

    public string? PersonName { get; set; }

    public NameChangeRequestInfo? NameChangeRequest { get; set; }

    public DateOfBirthChangeRequestInfo? DateOfBirthChangeRequest { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var processContext = new ProcessContext(
            ChangeType is SupportTaskType.ChangeNameRequest ? ProcessType.ChangeOfNameRequestApproving : ProcessType.ChangeOfDateOfBirthRequestApproving,
            timeProvider.UtcNow,
            User.GetUserId());

        await changeRequestSupportTaskService.ApproveChangeRequestAsync(
            new ApproveChangeRequestSupportTaskOptions { SupportTask = _supportTask! },
            processContext);

        TempData.SetFlashNotificationBanner(
            "The request has been accepted",
            "The user’s record has been changed and they have been notified.");

        return Redirect(linkGenerator.SupportTasks.ChangeRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        PersonName = string.JoinNonEmpty(
            ' ',
            _supportTask.Person!.FirstName,
            _supportTask.Person.MiddleName,
            _supportTask.Person.LastName);

        ChangeType = _supportTask.SupportTaskType;

        if (ChangeType is SupportTaskType.ChangeNameRequest)
        {
            var data = _supportTask.GetData<ChangeNameRequestData>();

            NameChangeRequest = new NameChangeRequestInfo
            {
                CurrentFirstName = _supportTask.Person.FirstName,
                CurrentMiddleName = _supportTask.Person.MiddleName,
                CurrentLastName = _supportTask.Person.LastName,
                NewFirstName = data.FirstName,
                NewMiddleName = data.MiddleName,
                NewLastName = data.LastName
            };
        }
        else
        {
            Debug.Assert(ChangeType is SupportTaskType.ChangeDateOfBirthRequest);

            var data = _supportTask.GetData<ChangeDateOfBirthRequestData>();

            DateOfBirthChangeRequest = new DateOfBirthChangeRequestInfo
            {
                CurrentDateOfBirth = _supportTask.Person.DateOfBirth!.Value,
                NewDateOfBirth = data.DateOfBirth
            };
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
