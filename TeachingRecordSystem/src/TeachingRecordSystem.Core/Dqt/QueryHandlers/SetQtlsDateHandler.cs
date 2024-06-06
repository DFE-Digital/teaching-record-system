using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class SetQtlsDateHandler : ICrmQueryHandler<SetQtlsDateQuery, bool>
{
    public async Task<bool> Execute(SetQtlsDateQuery query, IOrganizationServiceAsync organizationService)
    {
        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        if (query.HasActiveSanctions == true)
        {
            requestBuilder.AddRequest(new CreateRequest()
            {
                Target = new CrmTask()
                {
                    RegardingObjectId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
                    Category = "QTLS date set/removed for record with an active alert",
                    Description = !query.QtlsDate.HasValue ? "QTLSDate removed for a record with active alert" : $"QTLSDate {query.QtlsDate} set for a record with active alert",
                    Subject = "Notification for SET QTLS data collections team",
                    ScheduledEnd = query.TaskScheduleEnd
                }
            });
        }

        requestBuilder.AddRequest(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = query.ContactId,
                dfeta_qtlsdate = query.QtlsDate.ToDateTimeWithDqtBstFix(isLocalTime: false)
            }
        });
        await requestBuilder.Execute();

        return true;
    }
}
