using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record CreateNameChangeRequestCommand
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}

public class CreateNameChangeRequestHandler(ICrmQueryDispatcher crmQueryDispatcher, IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");

    public async Task Handle(CreateNameChangeRequestCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactByTrnQuery(command.Trn, new Microsoft.Xrm.Sdk.Query.ColumnSet()));

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

        var lastName = command.LastName;
        var firstAndMiddleNames = $"{command.FirstName} {command.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames[0];
        var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));

        await crmQueryDispatcher.ExecuteQuery(new CreateNameChangeIncidentQuery()
        {
            ContactId = contact.Id,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            StatedFirstName = command.FirstName,
            StatedMiddleName = command.MiddleName,
            StatedLastName = command.LastName,
            EvidenceFileName = command.EvidenceFileName,
            EvidenceFileContent = await evidenceFileResponse.Content.ReadAsStreamAsync(),
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        });
    }
}
