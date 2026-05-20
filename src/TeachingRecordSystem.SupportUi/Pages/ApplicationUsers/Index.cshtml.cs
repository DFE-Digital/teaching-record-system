using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ApplicationUsers;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel(TrsDbContext dbContext) : PageModel
{
    public ApplicationUserInfo[]? Users { get; set; }

    public async Task OnGetAsync()
    {
        Users = await dbContext.ApplicationUsers
            .OrderBy(u => u.Name)
            .Select(u => new ApplicationUserInfo(u.UserId, u.Name))
            .ToArrayAsync();
    }

    public record ApplicationUserInfo(Guid UserId, string Name);
}
