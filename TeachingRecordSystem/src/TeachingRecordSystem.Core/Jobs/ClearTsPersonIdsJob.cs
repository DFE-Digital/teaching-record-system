using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Core.Jobs;

public class ClearTsPersonIdsJob(IOrganizationServiceAsync2 organizationService)
{
    public void Execute(CancellationToken cancellationToken)
    {
        var serviceContext = new DqtCrmServiceContext(organizationService);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var withTsPersonId = serviceContext.ContactSet.Where(c => c.dfeta_TSPersonID != null).Take(50).ToArray();

            if (withTsPersonId.Length == 0)
            {
                break;
            }

            var builder = RequestBuilder.CreateMultiple(organizationService);

            foreach (var contact in withTsPersonId)
            {
                builder.AddRequest(new UpdateRequest()
                {
                    Target = new Contact() { Id = contact.Id, dfeta_TSPersonID = null }
                });
            }

#pragma warning disable VSTHRD002
            builder.ExecuteAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        }
    }
}
