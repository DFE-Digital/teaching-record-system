using System.Text;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class SetProfessionalStatusHandler(IClock clock) : ICrmQueryHandler<SetProfessionalStatusQuery, bool>
{
    public async Task<bool> ExecuteAsync(SetProfessionalStatusQuery query, IOrganizationServiceAsync organizationService)
    {
        var txnRequest = new ExecuteTransactionRequest()
        {
            ReturnResponses = true,
            Requests = new() { }
        };

        if (query.IsNewItt)
        {
            txnRequest.Requests.Add(new CreateRequest()
            {
                Target = query.Itt
            });
        }
        else
        {
            txnRequest.Requests.Add(new UpdateRequest()
            {
                Target = query.Itt
            });
        }

        if (query.InductionOutboxMessage is not null)
        {
            txnRequest.Requests.Add(new CreateRequest()
            {
                Target = query.InductionOutboxMessage
            });
        }

        if (query.IsNewQts)
        {
            txnRequest.Requests.Add(new CreateRequest()
            {
                Target = query.QtsRegistration
            });
        }
        else
        {
            txnRequest.Requests.Add(new UpdateRequest()
            {
                Target = query.QtsRegistration
            });
        }

        if (query.UpdateIttLinkToQts)
        {
            txnRequest.Requests.Add(new UpdateRequest()
            {
                Target = new dfeta_initialteachertraining()
                {
                    Id = query.Itt.Id,
                    dfeta_qtsregistration = query.QtsRegistration.ToEntityReference()
                }
            });
        }

        if (query.HasActiveAlert)
        {
            txnRequest.Requests.Add(new CreateRequest()
            {
                Target = CreateReviewTaskEntityForActiveSanctions(query.ContactId, query.Trn)
            });
        }

        _ = await organizationService.ExecuteAsync(txnRequest);
        return true;
    }

    public Models.Task CreateReviewTaskEntityForActiveSanctions(Guid contactId, string trn)
    {
        var description = GetDescription();

        return new Models.Task()
        {
            RegardingObjectId = contactId.ToEntityReference(Contact.EntityLogicalName),
            Category = "Notification for QTS unit - Register: matched record holds active sanction",
            Subject = "Register: active sanction match",
            Description = description,
            ScheduledEnd = clock.UtcNow
        };

        string GetDescription()
        {
            var sb = new StringBuilder();
            sb.Append($"Active sanction found: TRN {trn}");
            return sb.ToString();
        }
    }
}
