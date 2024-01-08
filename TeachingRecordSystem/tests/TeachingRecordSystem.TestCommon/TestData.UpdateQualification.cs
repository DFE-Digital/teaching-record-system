using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task DeleteMandatoryQualification(Guid qualificationId, string trsDeletedEvent, bool? syncEnabled = null)
    {
        await OrganizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = qualificationId,
                dfeta_TrsDeletedEvent = trsDeletedEvent,
                StateCode = dfeta_qualificationState.Inactive
            }
        });

        await SyncConfiguration.SyncIfEnabled(
            helper => helper.SyncMandatoryQualification(qualificationId, CancellationToken.None),
            syncEnabled);
    }
}
