using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateTrnRequestTaskHandler : ICrmQueryHandler<CreateTrnRequestTaskQuery, Guid>
{
    public async Task<Guid> ExecuteAsync(CreateTrnRequestTaskQuery query, IOrganizationServiceAsync organizationService)
    {
        var crmTask = new CrmTask()
        {
            Id = Guid.NewGuid(),
            Subject = CreateTrnRequestTaskQuery.TaskSubject,
            Description = query.Description,
            dfeta_EmailAddress = query.EmailAddress
        };

        var annotationBody = await StreamHelper.GetBase64EncodedFileContentAsync(query.EvidenceFileContent);

        var annotation = new Annotation()
        {
            ObjectId = crmTask.Id.ToEntityReference(CrmTask.EntityLogicalName),
            ObjectTypeCode = CrmTask.EntityLogicalName,
            Subject = query.EvidenceFileName,
            DocumentBody = annotationBody,
            MimeType = query.EvidenceFileMimeType,
            FileName = query.EvidenceFileName,
            NoteText = string.Empty
        };

        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        requestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = crmTask });
        requestBuilder.AddRequest(new CreateRequest() { Target = annotation });
        await requestBuilder.ExecuteAsync();

        return crmTask.Id;
    }
}
