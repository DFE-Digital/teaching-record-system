using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record CreateNameChangeRequestCommand
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

public class CreateNameChangeRequestHandler(ICrmQueryDispatcher crmQueryDispatcher, IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");

    public async Task<ApiResult<CreateNameChangeRequestResult>> HandleAsync(CreateNameChangeRequestCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(command.Trn, new Microsoft.Xrm.Sdk.Query.ColumnSet()));

        if (contact is null)
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

        var lastName = command.LastName;
        var firstAndMiddleNames = $"{command.FirstName} {command.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames[0];
        var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));

        var (_, ticketNumber) = await crmQueryDispatcher.ExecuteQueryAsync(new CreateNameChangeIncidentQuery()
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
            FromIdentity = true,
            EmailAddress = command.EmailAddress,
        });

        return new CreateNameChangeRequestResult(ticketNumber);
    }
}
