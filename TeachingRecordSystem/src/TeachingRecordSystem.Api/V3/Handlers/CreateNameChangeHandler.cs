using MediatR;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class CreateNameChangeHandler : IRequestHandler<CreateNameChangeRequest>
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly HttpClient _downloadEvidenceFileHttpClient;

    public CreateNameChangeHandler(
        ICrmQueryDispatcher crmQueryDispatcher,
        IHttpClientFactory httpClientFactory)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");
    }

    public async Task Handle(CreateNameChangeRequest request, CancellationToken cancellationToken)
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

        var lastName = request.LastName;
        var firstAndMiddleNames = $"{request.FirstName} {request.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames[0];
        var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));

        await _crmQueryDispatcher.ExecuteQuery(new CreateNameChangeIncidentQuery()
        {
            ContactId = contact.Id,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            StatedFirstName = request.FirstName,
            StatedMiddleName = request.MiddleName,
            StatedLastName = request.LastName,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileContent = await evidenceFileResponse.Content.ReadAsStreamAsync(),
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        });
    }
}
