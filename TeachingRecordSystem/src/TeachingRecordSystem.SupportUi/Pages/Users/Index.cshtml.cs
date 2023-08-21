using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

    public User[]? AllUsers { get; set; }

    public async Task OnGet()
    {
        AllUsers = await _dbContext.Users.Where(u => u.UserType == UserType.Person && !string.IsNullOrWhiteSpace(u.Email)).OrderBy(u => u.Name).ToArrayAsync();
    }
}
