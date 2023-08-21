using MediatR;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class CreateDateOfBirthChangeHandler : IRequestHandler<CreateDateOfBirthChangeRequest>
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly HttpClient _downloadEvidenceFileHttpClient;

    public CreateDateOfBirthChangeHandler(
        ICrmQueryDispatcher crmQueryDispatcher,
        IHttpClientFactory httpClientFactory)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");
    }

    public async Task Handle(CreateDateOfBirthChangeRequest request, CancellationToken cancellationToken)
    {
        var contact = await _crmQueryDispatcher.ExecuteQuery(
            new GetContactByTrnQuery(request.Trn, new Microsoft.Xrm.Sdk.Query.ColumnSet()));

        if (contact is null)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }

        using var evidenceFileResponse = await _downloadEvidenceFileHttpClient.GetAsync(
            request.EvidenceFileUrl,
            HttpCompletionOption.ResponseHeadersRead);

        if (!evidenceFileResponse.IsSuccessStatusCode)
        {
            throw new ErrorException(ErrorRegistry.SpecifiedUrlDoesNotExist());
        }

        var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

        if (!fileExtensionContentTypeProvider.TryGetContentType(request.EvidenceFileName, out var evidenceFileMimeType))
        {
            evidenceFileMimeType = "application/octet-stream";
        }

        await _crmQueryDispatcher.ExecuteQuery(new CreateDateOfBirthChangeIncidentQuery()
        {
            ContactId = contact.Id,
            DateOfBirth = request.DateOfBirth,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileContent = await evidenceFileResponse.Content.ReadAsStreamAsync(),
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        });
    }
}
