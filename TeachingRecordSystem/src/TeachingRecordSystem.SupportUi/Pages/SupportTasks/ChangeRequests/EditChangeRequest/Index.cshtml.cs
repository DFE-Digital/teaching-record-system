using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest;

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

    public string? Email { get; set; }

    public string? Trn { get; set; }

    public Guid? PersonId { get; set; }

    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetEvidenceAsync()
    {
        var stream = await fileService.OpenReadStreamAsync(Evidence!.FileId);
        return File(stream, Evidence.MimeType);
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

        PersonName = StringExtensions.JoinNonEmpty(
            ' ',
            person.FirstName,
            person.MiddleName,
            person.LastName);

        Trn = person.Trn;
        PersonId = person.PersonId;
        ChangeType = supportTask.SupportTaskType;
        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
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
            Email = !string.IsNullOrEmpty(data.EmailAddress) ? data.EmailAddress : person.EmailAddress;

            if (!fileExtensionContentTypeProvider.TryGetContentType(data.EvidenceFileName, out var evidenceFileMimeType))
            {
                evidenceFileMimeType = "application/octet-stream";
            }

            Evidence = new EvidenceInfo()
            {
                FileId = data.EvidenceFileId,
                FileName = data.EvidenceFileName,
                FileUrl = await fileService.GetFileUrlAsync(data.EvidenceFileId, WebConstants.FileUrlExpiry),
                MimeType = evidenceFileMimeType
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
            Email = !string.IsNullOrEmpty(data.EmailAddress) ? data.EmailAddress : person.EmailAddress;

            if (!fileExtensionContentTypeProvider.TryGetContentType(data.EvidenceFileName, out var evidenceFileMimeType))
            {
                evidenceFileMimeType = "application/octet-stream";
            }

            Evidence = new EvidenceInfo()
            {
                FileId = data.EvidenceFileId,
                FileName = data.EvidenceFileName,
                FileUrl = await fileService.GetFileUrlAsync(data.EvidenceFileId, WebConstants.FileUrlExpiry),
                MimeType = evidenceFileMimeType
            };
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record EvidenceInfo
    {
        public required Guid FileId { get; init; }
        public required string FileName { get; init; }
        public required string FileUrl { get; init; }
        public required string MimeType { get; init; }
    }
}
