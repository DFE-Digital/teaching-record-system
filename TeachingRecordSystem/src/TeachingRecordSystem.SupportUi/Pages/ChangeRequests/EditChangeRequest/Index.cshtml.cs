using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.SupportTasksEdit)]
public class IndexModel(
    TrsDbContext dbContext,
    IFileService fileService) : PageModel
{
    public SupportTaskType? ChangeType { get; set; }

    public string? PersonName { get; set; }

    public NameChangeRequestInfo? NameChangeRequest { get; set; }

    public DateOfBirthChangeRequestInfo? DateOfBirthChangeRequest { get; set; }

    public EvidenceInfo? Evidence { get; set; }

    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public void OnGet()
    {
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var person = await dbContext.Persons
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.PersonId == supportTask.PersonId);
        if (person is null)
        {
            context.Result = NotFound();
            return;
        }

        PersonName = StringHelper.JoinNonEmpty(
            ' ',
            person.FirstName,
            person.MiddleName,
            person.LastName);

        ChangeType = supportTask.SupportTaskType;
        if (supportTask.SupportTaskType == SupportTaskType.ChangeNameRequest)
        {
            var data = (ChangeNameRequestData)supportTask.Data;
            NameChangeRequest = new NameChangeRequestInfo()
            {
                CurrentFirstName = person!.FirstName,
                CurrentMiddleName = person.MiddleName,
                CurrentLastName = person.LastName,
                NewFirstName = data.FirstName,
                NewMiddleName = data.MiddleName,
                NewLastName = data.LastName
            };

            Evidence = new EvidenceInfo()
            {
                FileId = data.EvidenceFileId,
                FileName = data.EvidenceFileName,
                FileUrl = await fileService.GetFileUrlAsync(data.EvidenceFileId, FileUploadDefaults.FileUrlExpiry),
                IsPdf = data.EvidenceFileName?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ?? false
            };
        }

        if (supportTask.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest)
        {
            var data = (ChangeDateOfBirthRequestData)supportTask.Data;
            DateOfBirthChangeRequest = new DateOfBirthChangeRequestInfo()
            {
                CurrentDateOfBirth = person.DateOfBirth!.Value,
                NewDateOfBirth = data.DateOfBirth
            };

            Evidence = new EvidenceInfo()
            {
                FileId = data.EvidenceFileId,
                FileName = data.EvidenceFileName,
                FileUrl = await fileService.GetFileUrlAsync(data.EvidenceFileId, FileUploadDefaults.FileUrlExpiry),
                IsPdf = data.EvidenceFileName?.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ?? false
            };
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record EvidenceInfo
    {
        public required Guid FileId { get; init; }
        public required string FileName { get; init; }
        public required string FileUrl { get; init; }
        public required bool IsPdf { get; init; }
    }
}
