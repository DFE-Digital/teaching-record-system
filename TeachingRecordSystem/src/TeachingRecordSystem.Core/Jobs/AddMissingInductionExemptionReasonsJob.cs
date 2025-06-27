using System.Diagnostics;
using System.ServiceModel;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Jobs;

public class AddMissingInductionExemptionReasonsJob(TrsDbContext dbContext, IOrganizationServiceAsync2 organizationService, IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        var serviceContext = new DqtCrmServiceContext(organizationService);

        var contactsWithPassedInWales = serviceContext.ContactSet
            .Where(c => c.StateCode == ContactState.Active && c.dfeta_InductionStatus == dfeta_InductionStatus.PassedinWales)
            .Select(c => c.Id)
            .ToArray();

        foreach (var contactId in contactsWithPassedInWales)
        {
            var person = await dbContext.Persons.SingleAsync(p => p.PersonId == contactId, cancellationToken);

            if (person.InductionStatus is InductionStatus.Exempt &&
                !person.InductionExemptionReasonIds.Contains(InductionExemptionReason.PassedInWalesId))
            {
                person.AddInductionExemptionReason(
                    InductionExemptionReason.PassedInWalesId,
                    SystemUser.SystemUserId,
                    clock.UtcNow,
                    out var @event);
                Debug.Assert(@event is not null);

                if (person.InductionExemptWithoutReason)
                {
                    person.InductionExemptWithoutReason = false;

                    @event = @event with
                    {
                        Induction = @event.Induction with
                        {
                            InductionExemptWithoutReason = false
                        },
                        Changes = @event.Changes | PersonInductionUpdatedEventChanges.InductionExemptWithoutReason
                    };
                }

                dbContext.AddEventWithoutBroadcast(@event);

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            dbContext.ChangeTracker.Clear();
        }
    }
}
