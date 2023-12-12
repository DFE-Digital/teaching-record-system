using Microsoft.Crm.Sdk.Messages;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<Guid> GetCurrentCrmUserId()
    {
        var whoAmIResponse = (WhoAmIResponse)await OrganizationService.ExecuteAsync(new WhoAmIRequest());
        return whoAmIResponse.UserId;
    }
}
