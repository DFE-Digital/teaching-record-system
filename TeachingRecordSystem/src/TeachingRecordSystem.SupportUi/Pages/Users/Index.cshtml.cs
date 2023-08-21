using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Roles = UserRoles.Administrator)]
public class IndexModel : PageModel
{
    private readonly TrsDbContext _dbContext;

    public IndexModel(TrsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<User>? AllUsers { get; set; }

    public void OnGet()
    {
        AllUsers = _dbContext.Users.Where(u => u.UserType == UserType.Person).OrderBy(u => u.Name);
    }
}
