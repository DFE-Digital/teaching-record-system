using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public partial class CreateDateOfBirthChangeRequestHandler
{
    public async Task<ApiResult<CreateDateOfBirthChangeRequestResult>> HandleOverDqtAsync(CreateDateOfBirthChangeRequestCommand command)
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
