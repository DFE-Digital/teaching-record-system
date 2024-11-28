using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record CreateDateOfBirthChangeRequestCommand
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
    public required string? EmailAddress { get; init; }
}

public record CreateDateOfBirthChangeRequestResult(string CaseNumber);

public class CreateDateOfBirthChangeRequestHandler(ICrmQueryDispatcher crmQueryDispatcher, IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");

    public async Task<ApiResult<CreateDateOfBirthChangeRequestResult>> HandleAsync(CreateDateOfBirthChangeRequestCommand command)
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

        var (_, ticketNumber) = await crmQueryDispatcher.ExecuteQueryAsync(new CreateDateOfBirthChangeIncidentQuery()
        {
            ContactId = contact.Id,
            DateOfBirth = command.DateOfBirth,
            EvidenceFileName = command.EvidenceFileName,
            EvidenceFileContent = await evidenceFileResponse.Content.ReadAsStreamAsync(),
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true,
            EmailAddress = command.EmailAddress,
        });

        return new CreateDateOfBirthChangeRequestResult(ticketNumber);
    }
}
