using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record CreateDateOfBirthChangeRequestCommand
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}

public class CreateDateOfBirthChangeRequestHandler(ICrmQueryDispatcher crmQueryDispatcher, IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");

    public async Task Handle(CreateDateOfBirthChangeRequestCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQuery(
            new GetContactByTrnQuery(command.Trn, new Microsoft.Xrm.Sdk.Query.ColumnSet()));

        if (contact is null)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }

        using var evidenceFileResponse = await _downloadEvidenceFileHttpClient.GetAsync(
            command.EvidenceFileUrl,
            HttpCompletionOption.ResponseHeadersRead);

        if (!evidenceFileResponse.IsSuccessStatusCode)
        {
            throw new ErrorException(ErrorRegistry.SpecifiedUrlDoesNotExist());
        }

        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

        if (!fileExtensionContentTypeProvider.TryGetContentType(command.EvidenceFileName, out var evidenceFileMimeType))
        {
            evidenceFileMimeType = "application/octet-stream";
        }

        await crmQueryDispatcher.ExecuteQuery(new CreateDateOfBirthChangeIncidentQuery()
        {
            ContactId = contact.Id,
            DateOfBirth = command.DateOfBirth,
            EvidenceFileName = command.EvidenceFileName,
            EvidenceFileContent = await evidenceFileResponse.Content.ReadAsStreamAsync(),
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        });
    }
}
