using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Files;

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
    IConfiguration configuration,
    IBackgroundJobScheduler backgroundJobScheduler,
    IHttpClientFactory httpClientFactory,
    TrsDbContext dbContext,
    IFileService fileService,
    IClock clock) :
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

        using var stream = await evidenceFileResponse.Content.ReadAsStreamAsync();
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

        var getAnIdentityApplicationUserId = configuration.GetValue<Guid>("GetAnIdentityApplicationUserId");

        var supportTask = PostgresModels.SupportTask.Create(
            SupportTaskType.ChangeNameRequest,
            changeRequestData,
            person.PersonId,
            oneLoginUserSubject: null,
            trnRequestApplicationUserId: null,
            trnRequestId: null,
            createdBy: getAnIdentityApplicationUserId,
            clock.UtcNow,
            out var supportTaskCreatedEvent);

        dbContext.SupportTasks.Add(supportTask);
        await dbContext.AddEventAndBroadcastAsync(supportTaskCreatedEvent);

        var emailAddress = string.IsNullOrEmpty(command.EmailAddress) ? person.EmailAddress : command.EmailAddress;

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
            await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId));
        }

        await dbContext.SaveChangesAsync();

        return new CreateNameChangeRequestResult(supportTask.SupportTaskReference);
    }
}
