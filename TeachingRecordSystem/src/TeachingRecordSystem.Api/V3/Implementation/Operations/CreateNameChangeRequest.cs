using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record CreateNameChangeRequestCommand : ICommand<CreateNameChangeRequestResult>
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
    public required string? EmailAddress { get; init; }
}

public record CreateNameChangeRequestResult(string CaseNumber);

public class CreateNameChangeRequestHandler(
    IBackgroundJobScheduler backgroundJobScheduler,
    IHttpClientFactory httpClientFactory,
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    IFileService fileService,
    IClock clock,
    ICurrentUserProvider currentUserProvider) :
    ICommandHandler<CreateNameChangeRequestCommand, CreateNameChangeRequestResult>
{
    private readonly HttpClient _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");

    public async Task<ApiResult<CreateNameChangeRequestResult>> ExecuteAsync(CreateNameChangeRequestCommand command)
    {
        var person = await dbContext.Persons
            .Where(p => p.Trn == command.Trn)
            .SingleOrDefaultAsync();

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var existingOpenRequest = await dbContext.SupportTasks
            .Where(t => t.PersonId == person.PersonId
                && t.SupportTaskType == SupportTaskType.ChangeNameRequest
                && (t.Status == SupportTaskStatus.Open || t.Status == SupportTaskStatus.InProgress))
            .AnyAsync();

        if (existingOpenRequest)
        {
            return ApiError.OpenChangeRequestAlreadyExists("change name");
        }

        using var evidenceFileResponse = await _downloadEvidenceFileHttpClient.GetAsync(
            command.EvidenceFileUrl,
            HttpCompletionOption.ResponseHeadersRead);

        if (!evidenceFileResponse.IsSuccessStatusCode)
        {
            return ApiError.SpecifiedResourceUrlDoesNotExist(command.EvidenceFileUrl);
        }

        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        if (!fileExtensionContentTypeProvider.TryGetContentType(command.EvidenceFileName, out var evidenceFileMimeType))
        {
            evidenceFileMimeType = "application/octet-stream";
        }

        await using var stream = await evidenceFileResponse.Content.ReadAsStreamAsync();
        var evidenceFileId = await fileService.UploadFileAsync(stream, evidenceFileMimeType);

        var changeRequestData = new ChangeNameRequestData()
        {
            FirstName = command.FirstName,
            MiddleName = command.MiddleName,
            LastName = command.LastName,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = command.EvidenceFileName,
            EmailAddress = command.EmailAddress,
            ChangeRequestOutcome = null
        };

        var userId = currentUserProvider.GetCurrentApplicationUserId();

        var processContext = new ProcessContext(ProcessType.ChangeOfNameRequestCreating, clock.UtcNow, userId);

        var supportTask = await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.ChangeNameRequest,
                Data = changeRequestData,
                PersonId = person.PersonId,
                OneLoginUserSubject = null,
                TrnRequest = null
            },
            processContext);

        var emailAddress = !string.IsNullOrEmpty(command.EmailAddress) ? command.EmailAddress : person.EmailAddress;

        if (!string.IsNullOrEmpty(emailAddress))
        {
            var email = new Email
            {
                EmailId = Guid.NewGuid(),
                TemplateId = EmailTemplateIds.GetAnIdentityChangeOfNameSubmittedEmailConfirmation,
                EmailAddress = emailAddress,
                Personalization = new Dictionary<string, string>() { { ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey, person.FirstName } }
            };

            dbContext.Emails.Add(email);

            await dbContext.SaveChangesAsync();

            await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId, processContext.ProcessId));
        }

        await dbContext.SaveChangesAsync();

        return new CreateNameChangeRequestResult(supportTask.SupportTaskReference);
    }
}
