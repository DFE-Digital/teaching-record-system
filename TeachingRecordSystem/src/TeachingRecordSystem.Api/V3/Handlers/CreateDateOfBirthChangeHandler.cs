using MediatR;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.Validation;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class CreateDateOfBirthChangeHandler : IRequestHandler<CreateDateOfBirthChangeRequest>
{
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly HttpClient _downloadEvidenceFileHttpClient;

    public CreateDateOfBirthChangeHandler(
        IDataverseAdapter dataverseAdapter,
        IHttpClientFactory httpClientFactory)
    {
        _dataverseAdapter = dataverseAdapter;
        _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");
    }

    public async Task Handle(CreateDateOfBirthChangeRequest request, CancellationToken cancellationToken)
    {
        var person = await _dataverseAdapter.GetTeacherByTrn(request.Trn, columnNames: Array.Empty<string>());

        if (person is null)
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

        var command = new CreateDateOfBirthChangeIncidentCommand()
        {
            ContactId = person.Id,
            Trn = request.Trn,
            DateOfBirth = request.DateOfBirth,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileContent = await evidenceFileResponse.Content.ReadAsStreamAsync(),
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        };

        await _dataverseAdapter.CreateDateOfBirthChangeIncident(command);
    }
}
