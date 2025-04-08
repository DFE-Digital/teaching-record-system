using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
public class IndexModel : PageModel
{
    private readonly TrsDbContext _dbContext;

    public IndexModel(TrsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public User[]? AllUsers { get; set; }

    public async Task OnGetAsync()
    {
        AllUsers = await _dbContext.Users.Where(u => u.UserType == UserType.Person && !string.IsNullOrWhiteSpace(u.Email)).OrderBy(u => u.Name).ToArrayAsync();
    }
}
