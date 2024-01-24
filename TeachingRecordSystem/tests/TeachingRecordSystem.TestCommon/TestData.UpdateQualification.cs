using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task DeleteMandatoryQualification(Guid qualificationId, MandatoryQualificationDeletedEvent @event, bool? syncEnabled = null)
    {
        await OrganizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = qualificationId,
                dfeta_TRSEvent = EventInfo.Create(@event).Serialize(),
                StateCode = dfeta_qualificationState.Inactive
            }
        });

        await SyncConfiguration.SyncIfEnabled(
            helper => helper.SyncMandatoryQualification(qualificationId, events: [@event], CancellationToken.None),
            syncEnabled);
    }
}
