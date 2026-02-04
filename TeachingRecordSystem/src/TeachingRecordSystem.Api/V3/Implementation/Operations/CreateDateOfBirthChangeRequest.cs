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

public record CreateDateOfBirthChangeRequestCommand : ICommand<CreateDateOfBirthChangeRequestResult>
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
    public required string? EmailAddress { get; init; }
}

public record CreateDateOfBirthChangeRequestResult(string CaseNumber);

public class CreateDateOfBirthChangeRequestHandler(
    IBackgroundJobScheduler backgroundJobScheduler,
    IHttpClientFactory httpClientFactory,
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    IFileService fileService,
    IClock clock,
    ICurrentUserProvider currentUserProvider) :
    ICommandHandler<CreateDateOfBirthChangeRequestCommand, CreateDateOfBirthChangeRequestResult>
{
    private readonly HttpClient _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");

    public async Task<ApiResult<CreateDateOfBirthChangeRequestResult>> ExecuteAsync(CreateDateOfBirthChangeRequestCommand command)
    {
        var person = await dbContext.Persons
            .Where(p => p.Trn == command.Trn)
            .SingleOrDefaultAsync();

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var existingOpenRequest = await dbContext.SupportTasks
            .AnyAsync(t => t.PersonId == person.PersonId
                && t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest
                && t.IsOutstanding);

        if (existingOpenRequest)
        {
            return ApiError.OpenChangeDateOfBirthRequestAlreadyExists();
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

        var changeRequestData = new ChangeDateOfBirthRequestData()
        {
            DateOfBirth = command.DateOfBirth,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = command.EvidenceFileName,
            EmailAddress = command.EmailAddress,
            ChangeRequestOutcome = null
        };

        var userId = currentUserProvider.GetCurrentApplicationUserId();

        var processContext = new ProcessContext(ProcessType.ChangeOfDateOfBirthRequestCreating, clock.UtcNow, userId);

        var supportTask = await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.ChangeDateOfBirthRequest,
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
                TemplateId = EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthSubmittedEmailConfirmation,
                EmailAddress = emailAddress,
                Personalization = new Dictionary<string, string>() { { ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey, person.FirstName } }
            };

            dbContext.Emails.Add(email);

            await dbContext.SaveChangesAsync();

            await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId, processContext.ProcessId));
        }

        return new CreateDateOfBirthChangeRequestResult(supportTask.SupportTaskReference);
    }
}
