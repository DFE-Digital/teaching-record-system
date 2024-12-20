using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<Guid> GetCurrentCrmUserIdAsync()
    {
        var whoAmIResponse = (WhoAmIResponse)await OrganizationService.ExecuteAsync(new WhoAmIRequest());
        return whoAmIResponse.UserId;
    }

    public async Task<EntityReference> GetCurrentCrmUserAsync()
    {
        var userId = await GetCurrentCrmUserIdAsync();
        var user = (SystemUser)await OrganizationService.RetrieveAsync(SystemUser.EntityLogicalName, userId, new ColumnSet(SystemUser.Fields.FullName));
        return new EntityReference(SystemUser.EntityLogicalName, userId) { Name = user.FullName };
    }
}
