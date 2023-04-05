#nullable disable
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.StaticFiles;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.V3.Requests;
using QualifiedTeachersApi.Validation;

namespace QualifiedTeachersApi.V3.Handlers;

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

    public async Task<Unit> Handle(CreateNameChangeRequest request, CancellationToken cancellationToken)
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

        var command = new CreateNameChangeIncidentCommand()
        {
            ContactId = person.Id,
            Trn = request.Trn,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            EvidenceFileName = request.EvidenceFileName,
            EvidenceFileContent = await evidenceFileResponse.Content.ReadAsStreamAsync(),
            EvidenceFileMimeType = evidenceFileMimeType
        };

        await _dataverseAdapter.CreateNameChangeIncident(command);

        return Unit.Value;
    }
}
