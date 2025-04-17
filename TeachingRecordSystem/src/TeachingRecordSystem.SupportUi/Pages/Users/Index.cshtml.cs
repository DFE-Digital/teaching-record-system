using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Users;

[Authorize(Policy = AuthorizationPolicies.UserManagement)]
[RequireFeatureEnabledFilterFactory(FeatureNames.NewUserRoles)]
public class IndexModel : PageModel
{
    private const int UsersPerPage = 10;

    private readonly TrsDbContext _dbContext;
    private readonly TrsLinkGenerator _linkGenerator;

    public IndexModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator)
    {
        _dbContext = dbContext;
        _linkGenerator = linkGenerator;
    }

    [FromQuery(Name = "page")]
    public int? CurrentPage { get; set; }

    private User[] AllUsers { get; set; } = [];

    public IEnumerable<UserViewModel> CurrentPageUsers { get; set; } = [];

    public PaginationViewModel? Pagination { get; set; }

    public bool HasUsers =>
        AllUsers.Length > 0;

    public async Task OnGetAsync()
    {
        AllUsers = await _dbContext.Users
            .Where(u => u.UserType == UserType.Person && !string.IsNullOrWhiteSpace(u.Email))
            .OrderBy(u => u.Name)
            .ToArrayAsync();

        Pagination = new PaginationViewModel(CurrentPage, AllUsers.Length, UsersPerPage, "");
        CurrentPageUsers = Pagination!
            .Paginate(AllUsers)
            .Select(CreateUserViewModel);
    }

    private UserViewModel CreateUserViewModel(User user)
    {
        return new()
        {
            Id = user.UserId.ToString(),
            Name = user.Name,
            EditUrl = _linkGenerator.LegacyEditUser(user.UserId),
            EmailAddress = user.Email ?? "(No email address)",
            Role = user.Role == null ? "(No user role assigned)" : UserRoles.GetDisplayNameForRole(user.Role),
            Status = user.Active ? "Active" : "Inactive"
        };
    }
}
