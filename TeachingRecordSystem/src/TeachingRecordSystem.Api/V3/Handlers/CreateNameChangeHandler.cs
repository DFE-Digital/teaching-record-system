using MediatR;
using Microsoft.AspNetCore.StaticFiles;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.Validation;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class CreateNameChangeHandler : IRequestHandler<CreateNameChangeRequest>
{
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly HttpClient _downloadEvidenceFileHttpClient;

    public CreateNameChangeHandler(
        IDataverseAdapter dataverseAdapter,
        IHttpClientFactory httpClientFactory)
    {
        _dataverseAdapter = dataverseAdapter;
        _downloadEvidenceFileHttpClient = httpClientFactory.CreateClient("EvidenceFiles");
    }

    public async Task Handle(CreateNameChangeRequest request, CancellationToken cancellationToken)
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

        var lastName = request.LastName;
        var firstAndMiddleNames = $"{request.FirstName} {request.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames[0];
        var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));

        var command = new CreateNameChangeIncidentCommand()
        {
            ContactId = person.Id,
            Trn = request.Trn,
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
        };

        await _dataverseAdapter.CreateNameChangeIncident(command);
    }
}
